using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.Core
{
    [TestClass]
    public class ServiceResolverTests
    {
        [TestMethod]
        public void FullNameExtractorHappyPathWithDefaults()
        {
            //ARRANGE
            var s = new ServiceResolver(new IServiceInstance[0], new ServiceResolverOptions());

            // ACT
            var fn = s.GetRegisteredNameFromFullName("Gtt.CodeWorks.Service.ServiceName");
            // ASSERT
            Assert.AreEqual("Gtt/CodeWorks/Service/ServiceName", fn);
        }

        [TestMethod]
        public void FullNameExtractorHappyPathMaxSegments_One()
        {
            //ARRANGE
            var s = new ServiceResolver(new IServiceInstance[0], new ServiceResolverOptions
            {
                NamespaceDepth = 1
            });

            // ACT
            var fn = s.GetRegisteredNameFromFullName("Gtt.CodeWorks.Service.ServiceName");
            // ASSERT
            Assert.AreEqual("Service/ServiceName", fn);
        }

        [TestMethod]
        public void FullNameExtractorHappyPathMaxSegments_Max()
        {
            //ARRANGE
            var s = new ServiceResolver(new IServiceInstance[0], new ServiceResolverOptions
            {
                NamespaceDepth = 10
            });

            // ACT
            var fn = s.GetRegisteredNameFromFullName("Gtt.CodeWorks.Service.ServiceName");
            // ASSERT
            Assert.AreEqual("Gtt/CodeWorks/Service/ServiceName", fn);
        }

        [TestMethod]
        public void FullNameExtractoIgnore()
        {
            //ARRANGE
            var s = new ServiceResolver(new IServiceInstance[0], new ServiceResolverOptions
            {
                NamespacePrefixToIgnore = "Gtt.CodeWorks"
            });

            // ACT
            var fn = s.GetRegisteredNameFromFullName("Gtt.CodeWorks.Service.ServiceName");
            // ASSERT
            Assert.AreEqual("Service/ServiceName", fn);
        }

        [TestMethod]
        public void FullNameExtractoIgnoreDifferentSeparator()
        {
            //ARRANGE
            var s = new ServiceResolver(new IServiceInstance[0], new ServiceResolverOptions
            {
                NamespacePrefixToIgnore = "Gtt.CodeWorks",
                NamespaceSeparator = "-"
            });

            // ACT
            var fn = s.GetRegisteredNameFromFullName("Gtt.CodeWorks.Service.ServiceName");
            // ASSERT
            Assert.AreEqual("Service-ServiceName", fn);
        }
    }
}
