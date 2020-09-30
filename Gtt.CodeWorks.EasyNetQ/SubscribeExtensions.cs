using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private const int DefaultRetryAttempts = 5;
        private const int DefaultRetryDelay = 5 * 1000; // FIVE SECONDS
        private const int MaxRetryDelay = 60 * 1000; // ONE MINUTE

        private static Tuple<IQueue, IExchange> CreateExceptionQueues(IBus bus, ISubscriptionResult subscription)
        {
            var eq = bus.Advanced.QueueDeclare($"{subscription.Queue.Name}{Exceptions}");
            var ee = bus.Advanced.ExchangeDeclare($"{subscription.Queue.Name}{Exceptions}", ExchangeType);
            bus.Advanced.Bind(ee, eq, "");
            return Tuple.Create(eq, ee);
        }

        private static Tuple<IExchange, IExchange, IQueue> CreateRetryQueues(IBus bus, ISubscriptionResult subscription, int retryDelay)
        {
            var re = bus.Advanced.ExchangeDeclare($"{subscription.Queue.Name}{Retry}", ExchangeType);
            var he = bus.Advanced.ExchangeDeclare($"{subscription.Queue.Name}{Holding}_{retryDelay}ms", ExchangeType);
            var fe = bus.Advanced.ExchangeDeclare($"{subscription.Queue.Name}{Failed}", ExchangeType);

            var hq = bus.Advanced.QueueDeclare($"{subscription.Queue.Name}{Holding}_{retryDelay}ms", perQueueMessageTtl: retryDelay, deadLetterExchange: re.Name);
            var fq = bus.Advanced.QueueDeclare($"{subscription.Queue.Name}{Failed}");

            bus.Advanced.Bind(re, subscription.Queue, "");
            bus.Advanced.Bind(he, hq, "");
            bus.Advanced.Bind(fe, fq, "");

            return Tuple.Create(he, fe, fq);
        }

        public static void SubscribeWithRetry<T>(this IBus bus,
            string subscriptionId,
            Action<T> handler,
            Action<T> failedHandler = null) where T : class
        {
            SubscribeWithRetry(bus, subscriptionId, DefaultRetryAttempts, DefaultRetryDelay, handler, failedHandler);
        }

        public static void SubscribeWithRetry<T>(this IBus bus,
            string subscriptionId,
            int retryAttempts,
            int retryDelay,
            Action<T> handler,
            Action<T> failedHandler = null) where T : class
        {
            if (retryDelay < 0 || retryDelay > MaxRetryDelay)
            {
                throw new ArgumentException($"retryDelay if defined must be between 0 and {MaxRetryDelay}", nameof(retryDelay));
            }

            bus.Advanced.Conventions.ErrorExchangeNamingConvention = m => $"{m.Queue}{Exceptions}";
            bus.Advanced.Conventions.ErrorQueueNamingConvention = m => $"{m.Queue}{Exceptions}";

            //SETUP SUBSCRIBER
            var subscription = bus.Subscribe(subscriptionId, handler);
            var ex = CreateExceptionQueues(bus, subscription); // PROACTIVELY CREATE EXCEPTION QUEUES
            var re = CreateRetryQueues(bus, subscription, retryDelay); // CREATE RETRY & HOLDING QUEUES

            bus.Advanced.Consume<Error>(ex.Item1, (msg, data) =>
            {
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

                if (rv < retryAttempts)
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

        public static void CleanUpQueuesAndExchanges<T>(this IBus bus, string subscriberId, bool deletePrimaryExchange = false)
        {
            CleanUpQueuesAndExchanges<T>(bus, subscriberId, DefaultRetryDelay, deleteMode: DeleteMode.EverythingExceptTypeExchange);
        }

        public static void CleanUpQueuesAndExchanges<T>(this IBus bus, string subscriberId, int retryDelay, DeleteMode deleteMode)
        {
            var qConventions = bus.Advanced.Conventions.QueueNamingConvention;
            var queueName = qConventions(typeof(T), subscriberId);
            var eConvention = bus.Advanced.Conventions.ExchangeNamingConvention;

            switch (deleteMode)
            {
                case DeleteMode.EverythingExceptTypeExchange:
                    bus.Advanced.QueueDelete(new Queue($"{queueName}{Exceptions}", isExclusive: false));
                    bus.Advanced.QueueDelete(new Queue($"{queueName}{Failed}", isExclusive: false));
                    bus.Advanced.ExchangeDelete(new Exchange($"{queueName}{Retry}"));
                    bus.Advanced.ExchangeDelete(new Exchange($"{queueName}{Failed}"));
                    bus.Advanced.ExchangeDelete(new Exchange($"{queueName}{Exceptions}"));
                    bus.Advanced.ExchangeDelete(new Exchange($"{queueName}{Holding}_{retryDelay}ms"));
                    bus.Advanced.QueueDelete(new Queue($"{queueName}{Holding}_{retryDelay}ms", false));
                    bus.Advanced.QueueDelete(new Queue(queueName, isExclusive: false));
                    break;

                case DeleteMode.OnlyHoldingQueues:
                    bus.Advanced.QueueDelete(new Queue($"{queueName}{Failed}", isExclusive: false));
                    bus.Advanced.ExchangeDelete(new Exchange($"{queueName}{Retry}"));
                    bus.Advanced.ExchangeDelete(new Exchange($"{queueName}{Failed}"));
                    bus.Advanced.ExchangeDelete(new Exchange($"{queueName}{Holding}_{retryDelay}ms"));
                    bus.Advanced.QueueDelete(new Queue($"{queueName}{Holding}_{retryDelay}ms", false));
                    break;

                case DeleteMode.Everything:
                    bus.Advanced.QueueDelete(new Queue($"{queueName}{Exceptions}", isExclusive: false));
                    bus.Advanced.QueueDelete(new Queue($"{queueName}{Failed}", isExclusive: false));
                    bus.Advanced.ExchangeDelete(new Exchange($"{queueName}{Holding}_{retryDelay}ms"));
                    bus.Advanced.QueueDelete(new Queue($"{queueName}{Holding}_{retryDelay}ms", false));
                    bus.Advanced.ExchangeDelete(new Exchange($"{queueName}{Retry}"));
                    bus.Advanced.ExchangeDelete(new Exchange($"{queueName}{Failed}"));
                    bus.Advanced.ExchangeDelete(new Exchange($"{queueName}{Exceptions}"));

                    bus.Advanced.QueueDelete(new Queue(queueName, isExclusive: false));
                    var exchangeName = eConvention(typeof(T));
                    bus.Advanced.ExchangeDelete(new Exchange(exchangeName));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(deleteMode), deleteMode, null);
            }
        }
    }
}
