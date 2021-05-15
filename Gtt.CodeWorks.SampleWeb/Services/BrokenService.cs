using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.SampleWeb.Services
{
    public class BrokenService : BaseServiceInstance<BrokenService.Request, BrokenService.Response>
    {
        public class Request : BaseRequest
        {

        }

        public class Response
        {

        }

        public BrokenService(CoreDependencies coreDependencies) : base(coreDependencies)
        {
        }

        protected override Task<ServiceResponse<Response>> Implementation(Request request, CancellationToken cancellationToken)
        {
            return Task.FromResult(ErrorCode(101));
        }

        protected override Task<string> CreateDistributedLockKey(Request request, CancellationToken cancellationToken)
        {
            return NoDistributedLock();
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return new Dictionary<int, string>
            {
                {100, "This is an error"}
            };
        }
    }
}
