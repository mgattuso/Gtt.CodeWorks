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
        private readonly ILocalClient<TimeService> _timeService;
        private readonly ILocalClient<SumService> _sumService;

        public CombinedService(
            ILocalClient<TimeService> timeService,
            ILocalClient<SumService> sumService,
            CoreDependencies coreDependencies) : base(coreDependencies)
        {
            _timeService = timeService;
            _sumService = sumService;
        }

        protected override async Task<ServiceResponse<CombinedResponse>> Implementation(CombinedRequest request, CancellationToken cancellationToken)
        {
            var time = await _timeService.Call<TimeRequest, TimeResponse>(this, new TimeRequest(), cancellationToken);
            var sum = await _sumService.Call<SumRequest, SumResponse>(this, new SumRequest
            {
                Number1 = 10,
                Number2 = 35
            }, cancellationToken);

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

        public override ServiceAction Action => ServiceAction.Create;
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
