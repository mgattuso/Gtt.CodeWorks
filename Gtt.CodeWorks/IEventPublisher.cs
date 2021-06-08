using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public interface IEventPublisher
    {
        Task Publish<T>(T @event,
            int retries = 3,
            int retryMs = 1000,
            bool exponential = true,
            TimeSpan? ttl = null,
            TimeSpan? delay = null) where T : PublishedEvent;
    }

    public class PublishedEvent : BaseRequest
    {
        public string EventId { get; set; }
        public PublishedEvent()
        {
            Published = ServiceClock.CurrentTime();
        }

        public EventAction Action { get; set; }
        public DateTimeOffset Published { get; set; }
    }

    public enum EventAction
    {
        Create,
        Update,
        Delete,
        Notify
    }

    public class NullEventPublisher : IEventPublisher
    {
        private NullEventPublisher()
        {
            
        }

        public Task Publish<T>(T @event, int retries = 3, int retryMs = 1000, bool exponential = true, TimeSpan? ttl = null,
            TimeSpan? delay = null) where T : PublishedEvent
        {
            Console.WriteLine($@"Publishing event using the Null Event Publisher for event {@event.CorrelationId}");
            return Task.CompletedTask;
        }

        public NullEventPublisher Default => new NullEventPublisher();

        
    }
}
