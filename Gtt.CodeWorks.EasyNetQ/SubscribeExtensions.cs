using System;
using System.Text;
using EasyNetQ;
using EasyNetQ.SystemMessages;
using EasyNetQ.Topology;

namespace Gtt.CodeWorks.EasyNetQ
{
    public static class SubscribeExtensions
    {
        private const string Exceptions = "_Exceptions";
        private const string Retry = "_Retry";
        private const string Holding = "_Holding";
        private const string Failed = "_Failed";
        private const string ExchangeType = "direct";
        private const string RetryHeader = "retry";

        private static Tuple<IQueue, IExchange> CreateExceptionQueues(IBus bus, ISubscriptionResult subscription)
        {
            var eq = bus.Advanced.QueueDeclare($"{subscription.Queue.Name}{Exceptions}");
            var ee = bus.Advanced.ExchangeDeclare($"{subscription.Queue.Name}{Exceptions}", ExchangeType);
            bus.Advanced.Bind(ee, eq, "");
            return Tuple.Create(eq, ee);
        }

        private static Tuple<IExchange, IExchange, IQueue> CreateRetryQueues(IBus bus, ISubscriptionResult subscription)
        {
            var re = bus.Advanced.ExchangeDeclare($"{subscription.Queue.Name}{Retry}", ExchangeType);
            var he = bus.Advanced.ExchangeDeclare($"{subscription.Queue.Name}{Holding}", ExchangeType);
            var fe = bus.Advanced.ExchangeDeclare($"{subscription.Queue.Name}{Failed}", ExchangeType);

            var hq = bus.Advanced.QueueDeclare($"{subscription.Queue.Name}{Holding}", perQueueMessageTtl: 5000, deadLetterExchange: re.Name);
            var fq = bus.Advanced.QueueDeclare($"{subscription.Queue.Name}{Failed}");

            bus.Advanced.Bind(re, subscription.Queue, "");
            bus.Advanced.Bind(he, hq, "");
            bus.Advanced.Bind(fe, fq, "");

            return Tuple.Create(he, fe, fq);
        }

        public static void SubscribeWithRetry<T>(this IBus bus, string subscriptionId, Action<T> handler, Action<T> failedHandler = null) where T : class
        {
            bus.Advanced.Conventions.ErrorExchangeNamingConvention = m => $"{m.Queue}{Exceptions}";
            bus.Advanced.Conventions.ErrorQueueNamingConvention = m => $"{m.Queue}{Exceptions}";

            //SETUP SUBSCRIBER
            var subscription = bus.Subscribe(subscriptionId, handler);
            var ex = CreateExceptionQueues(bus, subscription); // PROACTIVELY CREATE EXCEPTION QUEUES
            var re = CreateRetryQueues(bus, subscription); // CREATE RETRY & HOLDING QUEUES

            bus.Advanced.Consume<Error>(ex.Item1, (msg, data) =>
            {
                var serializer = bus.Advanced.Container.Resolve<IMessageSerializationStrategy>();
                var retryExisted = msg.Body.BasicProperties.Headers.TryGetValue(RetryHeader, out var retryAttempt);
                int rv = Convert.ToInt32(retryAttempt);
                rv++;
                if (!retryExisted)
                {
                    msg.Body.BasicProperties.Headers.Add(RetryHeader, rv);
                }
                else
                {
                    msg.Body.BasicProperties.Headers[RetryHeader] = rv;
                }
                if (rv < 5)
                {
                    bus.Advanced.Publish(re.Item1, "", true, msg.Body.BasicProperties, Encoding.ASCII.GetBytes(msg.Body.Message));
                }
                else
                {
                    bus.Advanced.Publish(re.Item2, "", true, msg);
                }
            });

            if (failedHandler != null)
            {
                bus.Advanced.Consume<Error>(re.Item3, (msg, msgInfo) =>
                {
                    var serializer = bus.Advanced.Container.Resolve<IMessageSerializationStrategy>();
                    var msgProps = msg.Body.BasicProperties;
                    var msgBody = msg.Body.Message;
                    var m = serializer.DeserializeMessage(msgProps, Encoding.UTF8.GetBytes(msgBody));
                    T obj = (T)m.GetBody();
                    failedHandler(obj);
                });
            }
        }
    }
}
