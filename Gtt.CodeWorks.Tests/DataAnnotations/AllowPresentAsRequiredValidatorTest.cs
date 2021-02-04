using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Gtt.CodeWorks.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.DataAnnotations
{
    [TestClass]
    public class AllowPresentAsRequiredValidatorTest
    {
        [TestMethod]
        public void HappyPath()
        {
            var dc = new DummyClass();

            DataAnnotationsValidator validator = new DataAnnotationsValidator(new ArrayCollectionPropertyNamingStrategy());
            var list = new List<ValidationResult>();

            var isValid = validator.TryValidateObjectRecursive(dc, list, new ValidationContext(dc));
            Assert.IsFalse(isValid);
            var err = list[0];
            Assert.AreEqual("The Message field is required", err.ErrorMessage);
        }

        public class DummyClass
        {
            [AlwaysPresent]
            public string Message { get; set; }
        }
    }
}
