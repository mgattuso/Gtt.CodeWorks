using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.SampleServices
{
    public class NotImplementedService : BaseServiceInstance<NotImplementedRequest, NotImplementedResponse>
    {
        public NotImplementedService(CoreDependencies coreDependencies) : base(coreDependencies)
        {
        }

        protected override Task<ServiceResponse<NotImplementedResponse>> Implementation(NotImplementedRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task<string> CreateDistributedLockKey(NotImplementedRequest request, CancellationToken cancellationToken)
        {
            return NoDistributedLock();
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }

    }

    public class NotImplementedRequest : BaseRequest
    {

    }

    public class NotImplementedResponse
    {

    }
}
