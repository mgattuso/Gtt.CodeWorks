using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.Core.Tokenizer
{
    [TestClass]
    public class ClientTokenStringTests
    {
        [TestMethod]
        public void HappyPathValueReturnsValue()
        {
            // arrange 
            ClientTokenString cts = "Hello";

            Assert.AreEqual("Hello", cts.OriginalValue);
            Assert.AreEqual("Hello", cts.ToString());
            Assert.AreEqual("", cts.MaskedValue);
            Assert.AreEqual(false, cts.IsTokenized());
        }

        [TestMethod]
        public void HappyPathTokenizedReturnsValue()
        {
            // arrange 
            ClientTokenString cts = "[T__ABC1234__T][M__****lo__M]";

            Assert.AreEqual("[T__ABC1234__T][M__****lo__M]", cts.OriginalValue);
            Assert.AreEqual("[T__ABC1234__T][M__****lo__M]", cts.ToString());
            Assert.AreEqual("****lo", cts.MaskedValue);
            Assert.AreEqual(true, cts.IsTokenized());
        }
    }

    [TestClass]
    public class ClientTokenDateTests
    {
        [TestMethod]
        public void HappyPathValueReturnsValue()
        {
            // arrange 
            ClientTokenDate cts = new DateTime(2002,3,4,5,6,7);

            Assert.AreEqual("2002-03-04T05:06:07.0000000", cts.OriginalValue);
            Assert.AreEqual("2002-03-04T05:06:07.0000000", cts.ToString());
            Assert.AreEqual("", cts.MaskedValue);
            Assert.AreEqual(false, cts.IsTokenized());
        }

        [TestMethod]
        public void HappyPathTokenOnlyReturnsValue()
        {
            // arrange 
            ClientTokenDate cts = "[T__ABC1234__T]";

            Assert.AreEqual("[T__ABC1234__T]", cts.OriginalValue);
            Assert.AreEqual("[T__ABC1234__T]", cts.ToString());
            Assert.AreEqual("", cts.MaskedValue);
            Assert.AreEqual("ABC1234", cts.TokenValue);
            Assert.AreEqual(true, cts.IsTokenized());
        }

        [TestMethod]
        public void HappyPathTokenizedReturnsValue()
        {
            // arrange 
            ClientTokenDate cts = "[T__ABC1234__T][M__05/**/2020__M]";

            Assert.AreEqual("[T__ABC1234__T][M__05/**/2020__M]", cts.OriginalValue);
            Assert.AreEqual("[T__ABC1234__T][M__05/**/2020__M]", cts.ToString());
            Assert.AreEqual("05/**/2020", cts.MaskedValue);
            Assert.AreEqual("ABC1234", cts.TokenValue);
            Assert.AreEqual(true, cts.IsTokenized());
        }
    }
}
