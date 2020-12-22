using System;
using System.Collections.Generic;
using System.Text;
using Gtt.CodeWorks.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.Core.Web
{
    [TestClass]
    public class CodeWorksClientBaseTests
    {
        [TestMethod]
        public void GetUrlHappyPath()
        {
            // arrange
            var harness = new HarnessClient("http://example.com/api/call");
            // act
            var url = harness.GetUrl("test");
            // assert
            Assert.AreEqual("http://example.com/api/call/test", url.ToString());
        }

        [TestMethod]
        public void GetUrlHappyPath_TrailingSlashOnRoot()
        {
            // arrange
            var harness = new HarnessClient("http://example.com/api/call/");
            // act
            var url = harness.GetUrl("test");
            // assert
            Assert.AreEqual("http://example.com/api/call/test", url.ToString());
        }

        [TestMethod]
        public void GetUrlHappyPathTrailingSlashOnRootAndLeadingSlashOnService()
        {
            // arrange
            var harness = new HarnessClient("http://example.com/api/call/");
            // act
            var url = harness.GetUrl("/test");
            // assert
            Assert.AreEqual("http://example.com/api/call/test", url.ToString());
        }

        [TestMethod]
        public void GetUrlHappyPath_LeadingSlashOnSegment()
        {
            // arrange
            var harness = new HarnessClient("http://example.com/api/call");
            // act
            var url = harness.GetUrl("/test");
            // assert
            Assert.AreEqual("http://example.com/api/call/test", url.ToString());
        }


        public class HarnessClient : CodeWorksClientEndpoint
        {
            public HarnessClient(string rootUrl, Dictionary<string, string> urlMap = null) : base(rootUrl, urlMap)
            {
            }

            public override Dictionary<string, string> ServiceRouteMap => new Dictionary<string, string>();
        }
    }
}
