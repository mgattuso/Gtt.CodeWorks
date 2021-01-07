using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gtt.CodeWorks.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.Core.Validation
{
    [TestClass]
    public class ValidationErrorResponseTests
    {
        [TestMethod]
        public void ToDictionaryWhenHasDuplicateKey()
        {
            // ARRANGE
            var ver = new ValidationErrorResponse();
            
            // ACT
            ver.AddValidationError("test", "Error");
            ver.AddValidationError("test", "Other error");

            var dict = ver.ToDictionary();

            // ASSERT
            Assert.AreEqual(1, dict.Count);
            var errorMessages = (object[])dict["test"];
            Assert.AreEqual(2, errorMessages.Length);
            Assert.IsTrue(errorMessages.Contains("Error"), "Contains error message=Error");
            Assert.IsTrue(errorMessages.Contains("Other error"), "Contains error message=Other error");
        }
    }
}
