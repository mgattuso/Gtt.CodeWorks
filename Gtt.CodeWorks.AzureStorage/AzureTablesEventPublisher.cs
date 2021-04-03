using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Gtt.CodeWorks.AzureStorage
{
    public class AzureTablesEventPublisher : IEventPublisher
    {
        private readonly IObjectSerializer _objectSerializer;
        private static readonly HashSet<string> InitializedQueues = new HashSet<string>();
        private readonly CloudQueueClient _client;

        public AzureTablesEventPublisher(string connectionString, IObjectSerializer objectSerializer)
        {
            _objectSerializer = objectSerializer;
            var account = CloudStorageAccount.Parse(connectionString);
            _client = account.CreateCloudQueueClient();
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
            var queue = _client.GetQueueReference(queueName);
            if (!InitializedQueues.Contains(queueName))
            {
                await queue.CreateIfNotExistsAsync();
                InitializedQueues.Add(queueName);
            }

            var msg = await _objectSerializer.Serialize(@event);

            IRetryPolicy retryPolicy = new NoRetry();
            if (retries > 0)
            {
                if (exponential)
                {
                    retryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(retryMs), retries);
                }
                else
                {
                    retryPolicy = new LinearRetry(TimeSpan.FromMilliseconds(retryMs), retries);
                }
            }

            await queue.AddMessageAsync(
                new CloudQueueMessage(msg),
                ttl,
                delay,
                new QueueRequestOptions
                {
                    RetryPolicy = retryPolicy
                }, new OperationContext
                {
                    ClientRequestID = @event.CorrelationId.ToString()
                }
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
