using System;
using System.Collections.Generic;
using System.Text;
using Gtt.CodeWorks.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.Core.Validation
{
    [TestClass]
    public class RequiredCollectionAttributeTests
    {

        [TestMethod]
        public void Null_Is_Not_Valid()
        {
            var rca = new RequiredCollectionAttribute();
            var result = rca.IsValid(null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Empty_Is_Not_Valid()
        {
            var rca = new RequiredCollectionAttribute();
            var result = rca.IsValid(new string[0]);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void One_Entry_Is_Valid_By_Default()
        {
            var rca = new RequiredCollectionAttribute();
            var result = rca.IsValid(new[] { "" });
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Below_Min_Entry_Is_Not_Valid()
        {
            var rca = new RequiredCollectionAttribute(2);
            var result = rca.IsValid(new[] { "" });
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Above_Max_Entry_Is_Not_Valid()
        {
            var rca = new RequiredCollectionAttribute(1, 2);
            var result = rca.IsValid(new[] { "", "", "" });
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Within_Min_Max_Entry_Is_Valid()
        {
            Assert.IsTrue(new RequiredCollectionAttribute(1, 2).IsValid(new[] { "" }));
            Assert.IsTrue(new RequiredCollectionAttribute(1, 2).IsValid(new[] { "","" }));
        }
    }
}

