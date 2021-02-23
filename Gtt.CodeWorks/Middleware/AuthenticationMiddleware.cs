using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Services;

namespace Gtt.CodeWorks.Middleware
{
    public class AuthenticationMiddleware : IServiceMiddleware
    {
        private readonly IUserResolver _userResolver;

        public AuthenticationMiddleware(IUserResolver userResolver)
        {
            _userResolver = userResolver;
        }

        public async Task<ServiceResponse> OnRequest<TReq>(IServiceInstance service, TReq request, CancellationToken cancellationToken) where TReq : BaseRequest, new()
        {


            if (service is IAuthenticatedServiceInstance authService)
            {
                var userResult = await _userResolver.GetUserOrDefault(request.AuthToken, request.CorrelationId, cancellationToken);

                if (authService.AllowAnonymous || authService.MustBeInRoles?.Length == 0)
                {
                    return this.ContinuePipeline();
                }

                switch (userResult.Status)
                {
                    case UserAuthStatus.Expired:
                        return new ServiceResponse(new ResponseMetaData(
                            service,
                            ServiceResult.NotAuthenticated, new ErrorData("Expired", "authToken")));

                    case UserAuthStatus.Invalid:
                        return new ServiceResponse(new ResponseMetaData(
                            service,
                            ServiceResult.ValidationError, new ErrorData("Invalid token", "authToken")));

                    case UserAuthStatus.Valid:
                        if (userResult.User == null)
                        {
                            throw new Exception("UserAuthStatus is valid but no user is provided");
                        }

                        if (!authService.UserIsAuthorized(userResult.User))
                        {
                            return new ServiceResponse(new ResponseMetaData(
                                service,
                                ServiceResult.NotAuthorized));
                        }

                        if (authService.MustBeInRoles != null && authService.MustBeInRoles.Length > 0)
                        {
                            if (userResult.User.Roles == null || userResult.User.Roles.Length == 0)
                            {
                                return new ServiceResponse(new ResponseMetaData(
                                    service,
                                    ServiceResult.NotAuthorized));
                            }

                            var lowerRoles = authService.MustBeInRoles.Where(x => x != null).Select(x => x.ToLowerInvariant().Trim());
                            var lowerUserRoles = authService.MustBeInRoles.Where(x => x != null).Select(x => x.ToLowerInvariant().Trim());

                            if (!lowerUserRoles.Intersect(lowerRoles).Any())
                            {
                                return new ServiceResponse(new ResponseMetaData(
                                    service,
                                    ServiceResult.NotAuthorized));
                            }
                        }

                        service.User = userResult.User;

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(userResult.Status), $"{userResult.Status} is not handled by AuthenticationMiddleware.OnRequest");
                }
            }

            return this.ContinuePipeline();
        }

        public Task OnResponse<TReq, TRes>(IServiceInstance service, TReq request, ServiceResponse<TRes> response,
            CancellationToken cancellationToken) where TReq : BaseRequest, new() where TRes : new()
        {
            return Task.CompletedTask;
        }

        public bool IgnoreExceptions => false;
        public bool SkipOnInternalCall => true;
    }
}
