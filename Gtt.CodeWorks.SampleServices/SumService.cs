using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.SampleServices
{
    public class SumService : BaseServiceInstance<SumRequest, SumResponse>
    {
        public SumService(CoreDependencies coreDependencies) : base(coreDependencies)
        {
        }

        protected override Task<ServiceResponse<SumResponse>> Implementation(SumRequest request, CancellationToken cancellationToken)
        {
            var response = new SumResponse
            {
                Sum = request.Number1 + request.Number2
            };

            return Task.FromResult(Successful(response));
        }

        protected override Task<string> CreateDistributedLockKey(SumRequest request, CancellationToken cancellationToken)
        {
            return NoDistributedLock();
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return AddErrorCodes(
                (99, "Out of range error"),
                (100, "Some other error")
            );
        }

        public override ServiceAction Action => ServiceAction.Create;
    }

    public class SumRequest : BaseRequest
    {
        public int Number1 { get; set; }
        public int Number2 { get; set; }
    }

    public class SumResponse
    {
        public double Sum { get; set; }
    }
}
