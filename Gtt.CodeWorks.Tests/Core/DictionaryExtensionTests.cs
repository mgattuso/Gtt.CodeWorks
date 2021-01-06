using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.Core
{
    [TestClass]
    public class DictionaryExtensionTests
    {
        [TestMethod]
        public void AddOrUpdate_HappyPath()
        {
            //arrange
            var d = new Dictionary<string, string[]>();

            // act
            d.AddOrAppendValue("test", "1");
            d.AddOrAppendValue("test", "2");
            d.AddOrAppendValue("test", "3");
            d.AddOrAppendValue("test", "4");
            d.AddOrAppendValue("test", null);

            // assert
            Assert.AreEqual(5, d["test"].Length);
        }
    }
}
