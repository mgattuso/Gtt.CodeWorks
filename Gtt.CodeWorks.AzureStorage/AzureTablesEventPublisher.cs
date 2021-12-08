using Azure.Storage.Queues;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.AzureStorage
{
    public class AzureTablesEventPublisher : IEventPublisher
    {
        private readonly IObjectSerializer _objectSerializer;
        private static readonly HashSet<string> InitializedQueues = new HashSet<string>();
        private readonly QueueClient _client;
        private string _connectionString;

        public AzureTablesEventPublisher(string connectionString, IObjectSerializer objectSerializer)
        {
            _connectionString = connectionString;
            _objectSerializer = objectSerializer;

        }

        public async Task Publish<T>(
            T @event,
            int retries = 3,
            int retryMs = 1000,
            bool exponential = true,
            TimeSpan? ttl = null,
            TimeSpan? delay = null) where T : PublishedEvent
        {
            if (delay != null && delay.Value.TotalMinutes > 7 * 24 * 60)
            {
                throw new Exception("delay can't be greater than 7 days");
            }

            if (delay != null && ttl != null && delay.Value.TotalMilliseconds > ttl.Value.TotalMilliseconds)
            {
                throw new Exception("delay can't be greater than the ttl");
            }

            var queueName = GetQueueNameFromEvent(typeof(T));

            var opts = new QueueClientOptions(QueueClientOptions.ServiceVersion.V2020_10_02);
            if (retries > 0)
            {
                opts.Retry.Delay = TimeSpan.FromMilliseconds(retryMs);
                opts.Retry.Mode = exponential ? Azure.Core.RetryMode.Exponential : Azure.Core.RetryMode.Fixed;
                opts.Retry.MaxRetries = retries;
            }

            var queue = new QueueClient(_connectionString, queueName, opts);
            if (!InitializedQueues.Contains(queueName))
            {
                await queue.CreateIfNotExistsAsync();
                InitializedQueues.Add(queueName);
            }

            var msg = await _objectSerializer.Serialize(@event);

            await queue.SendMessageAsync(
                msg,
                visibilityTimeout: delay,
                timeToLive: ttl
            );
        }

        public static string GetQueueNameFromEvent(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            Debug.Assert(t.FullName != null, "t.FullName != null");

            string typeName = t.FullName.ToLowerInvariant();
            string cleanName = Regex.Replace(typeName, @"[^\w\d]", "");
            if (cleanName.Length > 63)
            {
                cleanName = cleanName.Substring(cleanName.Length - 63, 63);
            }

            return cleanName;
        }
    }
}
