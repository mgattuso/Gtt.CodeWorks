using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.SampleServices
{
    public class PublishEventService : BaseServiceInstance<PublishEventRequest, PublishEventResponse>
    {
        private readonly IEventPublisher _eventPublisher;

        public PublishEventService(CoreDependencies coreDependencies, IEventPublisher eventPublisher) : base(coreDependencies)
        {
            _eventPublisher = eventPublisher;
        }

        protected override async Task<ServiceResponse<PublishEventResponse>> Implementation(PublishEventRequest request, CancellationToken cancellationToken)
        {
            await _eventPublisher.Publish(new TestEvent
            {
                Action = EventAction.Create,
                Message = request.Message,
                CorrelationId = request.CorrelationId
            },
                ttl: request.Ttl != null ? TimeSpan.FromMilliseconds(request.Ttl.Value) : (TimeSpan?)null,
                delay: request.Delay != null ? TimeSpan.FromMilliseconds(request.Delay.Value) : (TimeSpan?)null
                );

            var response = new PublishEventResponse();
            return Successful(response);
        }

        protected override Task<string> CreateDistributedLockKey(PublishEventRequest request, CancellationToken cancellationToken)
        {
            return NoDistributedLock();
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }

    }

    public class PublishEventRequest : BaseRequest
    {
        public int? Ttl { get; set; }
        public int? Delay { get; set; }
        public string Message { get; set; }
    }

    public class PublishEventResponse
    {

    }

    public class TestEvent : PublishedEvent
    {
        public TestEvent()
        {
            Action = EventAction.Create;
        }
        public string Message { get; set; }
    }
}
