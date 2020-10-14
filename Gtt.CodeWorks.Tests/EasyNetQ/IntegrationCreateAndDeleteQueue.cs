using System;
using EasyNetQ;
using Gtt.CodeWorks.EasyNetQ;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.EasyNetQ
{
    [TestClass]
    public class IntegrationCreateAndDeleteQueueTests
    {


        [TestMethod]
        public void Execute()
        {
            using (var bus = RabbitHutch.CreateBus("host=localhost"))
            {

                bus.SubscribeWithRetry<TestMessage>("a", 5, 1, msg =>
                {
                    Console.WriteLine($"{msg.Id}:{msg.Now} ---- {DateTime.Now}");
                    throw new Exception("Something wrong");
                });

                bus.CleanUpQueuesAndExchanges<TestMessage>("a", 1, DeleteMode.OnlyHoldingQueues);
            }
        }

        public class TestMessage
        {
            public DateTime Now { get; set; }
            public int Id { get; set; }
        }
    }
}
