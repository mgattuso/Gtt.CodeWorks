using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Validation;

namespace Gtt.CodeWorks.Middleware
{
    public class ValidationMiddleware : IServiceMiddleware
    {
        private readonly IRequestValidator _requestValidator;

        public ValidationMiddleware(IRequestValidator requestValidator)
        {
            _requestValidator = requestValidator;
        }

        public Task<ServiceResponse> OnRequest<TReq>(IServiceInstance service, TReq request, CancellationToken cancellationToken) where TReq : BaseRequest, new()
        {
            var validationResult = _requestValidator.Validate(request);
            if (validationResult.IsValid)
            {
                return Task.FromResult(this.ContinuePipeline());
            }

            return Task.FromResult(new ServiceResponse(new ResponseMetaData(service, validationResult.Errors)));
        }

        public Task OnResponse<TReq, TRes>(IServiceInstance service, TReq request, ServiceResponse<TRes> response,
            CancellationToken cancellationToken) where TReq : BaseRequest, new() where TRes : new()
        {
            return Task.CompletedTask;
        }

        public bool IgnoreExceptions => false;
        public bool SkipOnInternalCall => false;
    }
}
