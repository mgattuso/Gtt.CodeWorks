using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.SampleServices
{
    public class ConfirmValidationService : BaseServiceInstance<ConfirmValidationRequest, ConfirmValidationResponse>
    {
        public ConfirmValidationService(CoreDependencies coreDependencies) : base(coreDependencies)
        {
        }

        protected override Task<ServiceResponse<ConfirmValidationResponse>> Implementation(ConfirmValidationRequest request, CancellationToken cancellationToken)
        {
            var res = new ConfirmValidationResponse();
            return SuccessfulTask(res);
        }

        protected override Task<string> CreateDistributedLockKey(ConfirmValidationRequest request, CancellationToken cancellationToken)
        {
            return NoDistributedLock();
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }

        public override ServiceAction Action { get; }
    }

    public class ConfirmValidationRequest : BaseRequest
    {
        [Required]
        public string Password { get; set; }

        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

    public class ConfirmValidationResponse
    {

    }
}
