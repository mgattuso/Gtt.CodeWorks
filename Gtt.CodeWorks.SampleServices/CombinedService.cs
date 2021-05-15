using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Clients;

namespace Gtt.CodeWorks.SampleServices
{
    public class CombinedService : BaseServiceInstance<CombinedRequest, CombinedResponse>
    {
        private readonly TimeService _timeService;
        private readonly SumService _sumService;

        public CombinedService(
            TimeService timeService,
            SumService sumService,
            CoreDependencies coreDependencies) : base(coreDependencies)
        {
            _timeService = timeService;
            _sumService = sumService;
        }

        protected override async Task<ServiceResponse<CombinedResponse>> Implementation(CombinedRequest request, CancellationToken cancellationToken)
        {
            var time = await _timeService.ExecuteTakeCached(new TimeRequest(), cancellationToken);
            var sum = await _sumService.Execute(new SumRequest
            {
                Number1 = request.Number1,
                Number2 = request.Number2
            }, cancellationToken);

            var cachedTime = await _timeService.ExecuteTakeCached(new TimeRequest(), cancellationToken);
            var cachedSum = await _sumService.ExecuteTakeCached(new SumRequest(), cancellationToken);
            var noCachedSum = await _sumService.ExecuteTakeCached(new SumRequest
            {
                Number1 = request.Number1 * 2,
                Number2 = request.Number2 * 2
            }, x => x.Number1 == request.Number1, cancellationToken);

            var response = new CombinedResponse
            {
                Current = time.Data.CurrentTime,
                Sum = sum.Data.Sum,
            };
            return Successful(response);
        }

        protected override Task<string> CreateDistributedLockKey(CombinedRequest request, CancellationToken cancellationToken)
        {
            return NoDistributedLock();
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }

    }

    public class CombinedRequest : BaseRequest
    {
        public int Number1 { get; set; }
        public int Number2 { get; set; }
    }

    public class CombinedResponse
    {
        public DateTimeOffset Current { get; set; }
        public double Sum { get; set; }
    }
}
