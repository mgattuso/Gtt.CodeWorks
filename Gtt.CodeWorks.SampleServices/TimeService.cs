using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.SampleServices
{
    public class TimeService : BaseServiceInstance<TimeRequest, TimeResponse>
    {
        public TimeService(CoreDependencies coreDependencies) : base(coreDependencies)
        {
        }

        protected override async Task<ServiceResponse<TimeResponse>> Implementation(TimeRequest request, CancellationToken cancellationToken)
        {
            var time = ServiceClock.CurrentTime();
            var localTime = DateTimeOffset.Now;
            if (request.AddDelayMs > 0)
            {
                await Task.Delay(request.AddDelayMs, cancellationToken);
            }

            var response = new TimeResponse
            {
                CurrentTime = request.Zone == TimeZone.Local ? localTime : time,
                Zone = request.Zone
            };
            return Successful(response);
        }

        protected override Task<string> CreateDistributedLockKey(TimeRequest request, CancellationToken cancellationToken)
        {
            return NoDistributedLock();
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return AddErrorCodes(
                (100, "Time failed"),
                (101, "Something else Failed")
                );
        }

    }

    public class TimeRequest : BaseRequest
    {
        [Required]
        [Range(0, 30000)]
        public int AddDelayMs { get; set; }
        public TimeZone Zone { get; set; }
    }

    public enum TimeZone
    {
        [Description("Universal Coordinated Time")]
        UTC = 0,
        [Description("Local")]
        Local = 100
    }

    public class TimeResponse
    {
        public DateTimeOffset CurrentTime { get; set; }
        public TimeZone Zone { get; set; }
    }
}
