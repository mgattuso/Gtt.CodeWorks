using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.SampleServices
{
    public class TokenizedDataService : BaseServiceInstance<TokenizedDataRequest, TokenizedDataResponse>
    {
        public TokenizedDataService(CoreDependencies coreDependencies) : base(coreDependencies)
        {
        }

        protected override Task<ServiceResponse<TokenizedDataResponse>> Implementation(TokenizedDataRequest request, CancellationToken cancellationToken)
        {
            var response = new TokenizedDataResponse
            {
                Ssn = request.Ssn
            };
            return SuccessfulTask(response);
        }

        protected override Task<string> CreateDistributedLockKey(TokenizedDataRequest request, CancellationToken cancellationToken)
        {
            return NoDistributedLock();
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }

        public override ServiceAction Action => ServiceAction.Create;
    }

    public class TokenizedDataRequest : BaseRequest
    {
        [Required]
        public TokenString Ssn { get; set; }
    }

    public class TokenizedDataResponse
    {
        [AlwaysPresent]
        public TokenString Ssn { get; set; }
    }
}
