using System;
using System.Collections.Generic;
using System.Text;
using Gtt.CodeWorks.AzureStorage;
using Gtt.CodeWorks.ReallyReallyLongNamespace;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.AzureStorage
{
    [TestClass]
    public class AzureTablesEventPublisherTests
    {
        [TestMethod]
        public void GetQueueNameFromEvent_HappyPath()
        {
            // ACT
            var atep = AzureTablesEventPublisher.GetQueueNameFromEvent(typeof(TestClass));

            Assert.AreEqual("gttcodeworkstestsazurestoragetestclass", atep);

        }

        [TestMethod]
        public void GetQueueNameFromEvent_LongClassNameTrimsTotheRightSide()
        {
            // ACT
            var atep = AzureTablesEventPublisher.GetQueueNameFromEvent(typeof(TestClassWithAReallyLongNameThatPushesOverTheLengthLimit));

            Assert.AreEqual(63, atep.Length);
            Assert.AreEqual("mespacetestclasswithareallylongnamethatpushesoverthelengthlimit", atep);

        }
    }

    public class TestClass
    {

    }
}

namespace Gtt.CodeWorks.ReallyReallyLongNamespace
{
    public class TestClassWithAReallyLongNameThatPushesOverTheLengthLimit
    {

    }
}
