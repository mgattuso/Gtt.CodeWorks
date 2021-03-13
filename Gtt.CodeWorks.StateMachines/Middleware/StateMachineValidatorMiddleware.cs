using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Middleware;
using Gtt.CodeWorks.Validation;

namespace Gtt.CodeWorks.StateMachines.Middleware
{
    public class StateMachineValidatorMiddleware<TTrigger> : IServiceMiddleware
        where TTrigger : struct, IConvertible
    {
        private readonly IRequestValidator _requestValidator;

        public StateMachineValidatorMiddleware(IRequestValidator requestValidator)
        {
            _requestValidator = requestValidator;
        }

        public Task<ServiceResponse> OnRequest<TReq>(IServiceInstance service, TReq request, CancellationToken cancellationToken) where TReq : BaseRequest, new()
        {
            var stateMachineRequest = request as BaseStatefulRequest<TTrigger>;

            if (stateMachineRequest?.Trigger == null)
            {
                return Task.FromResult(this.ContinuePipeline());
            }

            var props = stateMachineRequest.GetType().GetProperties();
            var match = props.FirstOrDefault(x => x.Name.Equals(stateMachineRequest.Trigger.Value.ToString(), StringComparison.InvariantCultureIgnoreCase));

            if (match == null)
            {
                return Task.FromResult(this.ContinuePipeline());
            }

            var matchValue = match.GetValue(stateMachineRequest);

            if (matchValue == null)
            {
                return
                    Task.FromResult(
                        new ServiceResponse(new ResponseMetaData(
                                service, 
                                ServiceResult.ValidationError, 
                                validationErrors: ValidationHelper.Create("Is a required property", match.Name)))
                        );
            }

            var validationResult = _requestValidator.Validate(match.GetValue(stateMachineRequest), match.Name);
            if (validationResult.Count == 0)
            {
                return Task.FromResult(this.ContinuePipeline());
            }

            return Task.FromResult(new ServiceResponse(new ResponseMetaData(service, ServiceResult.ValidationError, validationErrors: validationResult)));
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
