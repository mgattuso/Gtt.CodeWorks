using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Gtt.CodeWorks.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.Core.Validation
{
    [TestClass]
    public class ValidEnumDefinedAttributeTests
    {
        [TestMethod]
        public void HappyPathError()
        {
            var results = new List<ValidationResult>();

            var request = new TestClassA
            {
                Test = (TestEnum)3
            };

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsFalse(isValid);
            Assert.AreEqual("Expected one of: First,Second", results[0].ErrorMessage);
        }

        [TestMethod]
        public void HappyPathSuccess()
        {
            var results = new List<ValidationResult>();

            var request = new TestClassA
            {
                Test = TestEnum.First
            };

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void HappyPathSuccessNullable()
        {
            var results = new List<ValidationResult>();

            var request = new TestClassA
            {
                Test = null
            };

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsTrue(isValid);
        }

        public class TestClassA
        {
            [ValidEnumDefined]
            public TestEnum? Test { get; set; }
        }

        public enum TestEnum
        {
            First,
            Second
        }
    }
}
