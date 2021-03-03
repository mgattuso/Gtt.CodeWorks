using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.SampleServices
{
    public class SignedInService : BaseServiceInstance<SignedInRequest, SignedInResponse>
    {
        public SignedInService(CoreDependencies coreDependencies) : base(coreDependencies)
        {
        }

        protected override Task<ServiceResponse<SignedInResponse>> Implementation(SignedInRequest request, CancellationToken cancellationToken)
        {
            var response = new SignedInResponse();
            return SuccessfulTask(response);
        }

        protected override Task<string> CreateDistributedLockKey(SignedInRequest request,
            CancellationToken cancellationToken)
        {
            return NoDistributedLock();
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }

        public override IAccessPolicy AccessPolicy => new LoggedInAccessPolicy();

        public override ServiceAction Action => ServiceAction.Read;
    }

    public class SignedInRequest : BaseRequest
    {

    }

    public class SignedInResponse
    {

    }


}
