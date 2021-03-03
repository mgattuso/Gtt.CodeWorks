using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Services;
using Microsoft.Extensions.Logging;

namespace Gtt.CodeWorks.Middleware
{
    public class AuthenticationMiddleware : IServiceMiddleware
    {
        private readonly ILogger<AuthenticationMiddleware> _logger;
        private readonly IUserResolver _userResolver;

        public AuthenticationMiddleware(IUserResolver userResolver, ILogger<AuthenticationMiddleware> logger)
        {
            _logger = logger;
            if (userResolver == null)
            {
                _logger.LogWarning("No userResolver providing using NullUserResolver");
            }
            _userResolver = userResolver ?? NullUserResolver.Instance;
        }

        public async Task<ServiceResponse> OnRequest<TReq>(IServiceInstance service, TReq request, CancellationToken cancellationToken) where TReq : BaseRequest, new()
        {
            if (service is IAuthenticatedServiceInstance authService)
            {
                if (string.IsNullOrWhiteSpace(request.AuthToken) && authService.AccessPolicy.IsAuthorized(null))
                {
                    _logger.LogTrace("No AuthToken provided and service allows non users to access. Continuing");
                    return this.ContinuePipeline();
                }

                _logger.LogTrace($"Found AuthToken:{request.AuthToken}, CorrelationId:{request.CorrelationId}");
                UserResolverResult userResult = await _userResolver.GetUserOrDefault(request.AuthToken, request.CorrelationId, cancellationToken);

                _logger.LogTrace($"Token Decrypted as: {userResult?.User?.Username} Status: {userResult?.Status}, CorrelationId:{request.CorrelationId}");

                if (userResult == null)
                {
                    _logger.LogTrace("Could not decrypt token successfully");
                    return new ServiceResponse(new ResponseMetaData(
                        service,
                        ServiceResult.NotAuthenticated));
                }

                switch (userResult.Status)
                {
                    case UserAuthStatus.NoUser:
                        return new ServiceResponse(new ResponseMetaData(
                            service,
                            ServiceResult.NotAuthenticated));

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

                        if (authService.AccessPolicy.IsAuthorized(userResult.User))
                        {
                            service.User = userResult.User;
                            return this.ContinuePipeline();
                        }

                        return new ServiceResponse(new ResponseMetaData(
                            service,
                            ServiceResult.NotAuthorized));

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
