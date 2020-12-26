using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Gtt.CodeWorks.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.Core.Validation
{
    [TestClass]
    public class DataAnnotationsValidatorTests
    {
        [TestMethod]
        public void ObjectWithNullCollectionIsValid()
        {
            var dav = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy());
            var request = new TestPerson { Name = "Test Person", PhoneNumbers = null };
            List<ValidationResult> results = new List<ValidationResult>();
            var isValid = dav.TryValidateObjectRecursive(request, results, new ValidationContext(request));
            Assert.IsTrue(isValid);
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void ObjectWithEmptyCollectionIsValid()
        {
            var dav = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy());
            var request = new TestPerson { Name = "Test Person", PhoneNumbers = new TestPhone[0] };
            List<ValidationResult> results = new List<ValidationResult>();
            var isValid = dav.TryValidateObjectRecursive(request, results, new ValidationContext(request));
            Assert.IsTrue(isValid);
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void ObjectWithOneInvalidEntryCollectionIsNotValid()
        {
            var dav = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy());
            var request = new TestPerson { Name = "Test Person", PhoneNumbers = new TestPhone[] { new TestPhone { Category = "home", Number = "" } } };
            List<ValidationResult> results = new List<ValidationResult>();
            var isValid = dav.TryValidateObjectRecursive(request, results, new ValidationContext(request));
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, results.Count);
        }

        [TestMethod]
        public void ObjectWithOneInvalidEntryCollectionsUsesIndexPositionInResult_DottedStrategy()
        {
            var dav = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy());
            var request = new TestPerson
            {
                Name = "Test Person",
                PhoneNumbers = new[]
            {
                new TestPhone { Category = "home", Number = "" }
            }
            };
            List<ValidationResult> results = new List<ValidationResult>();
            var isValid = dav.TryValidateObjectRecursive(request, results, new ValidationContext(request));
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("PhoneNumbers.0.Number", results[0].MemberNames.ToArray()[0]);
        }

        [TestMethod]
        public void ObjectWithOneInvalidEntryCollectionsUsesIndexPositionInResult_ArrayStrategy()
        {
            var dav = new DataAnnotationsValidator(new ArrayCollectionPropertyNamingStrategy());
            var request = new TestPerson
            {
                Name = "Test Person",
                PhoneNumbers = new[]
                {
                    new TestPhone { Category = "home", Number = "" }
                }
            };
            List<ValidationResult> results = new List<ValidationResult>();
            var isValid = dav.TryValidateObjectRecursive(request, results, new ValidationContext(request));
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("PhoneNumbers[0].Number", results[0].MemberNames.ToArray()[0]);
        }

        [TestMethod]
        public void ObjectWithOneInvalidEntryCollectionsUsesIndexPositionInResult_IgnoreIndexStrategy()
        {
            var dav = new DataAnnotationsValidator(new IgnoreIndexCollectionPropertyNamingStrategy());
            var request = new TestPerson
            {
                Name = "Test Person",
                PhoneNumbers = new[]
                {
                    new TestPhone { Category = "home", Number = "" }
                }
            };
            List<ValidationResult> results = new List<ValidationResult>();
            var isValid = dav.TryValidateObjectRecursive(request, results, new ValidationContext(request));
            Assert.IsFalse(isValid);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("PhoneNumbers.Number", results[0].MemberNames.ToArray()[0]);
        }

        [TestMethod]
        public void ObjectWithTwoInvalidEntryCollectionsUsesIndexPositionInResult()
        {
            var dav = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy());
            var request = new TestPerson
            {
                Name = "Test Person",
                PhoneNumbers = new[]
                {
                    new TestPhone { Category = "home", Number = "" },
                    new TestPhone { Category = "mobile", Number = "" }
                }
            };
            List<ValidationResult> results = new List<ValidationResult>();
            var isValid = dav.TryValidateObjectRecursive(request, results, new ValidationContext(request));
            Assert.IsFalse(isValid);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("PhoneNumbers.0.Number", results[0].MemberNames.ToArray()[0]);
            Assert.AreEqual("PhoneNumbers.1.Number", results[1].MemberNames.ToArray()[0]);
        }

        [TestMethod]
        public void ObjectWithTwoInvalidAndOneValidEntryCollectionsUsesIndexPositionInResult()
        {
            var dav = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy());
            var request = new TestPerson
            {
                Name = "Test Person",
                PhoneNumbers = new[]
                {
                    new TestPhone { Category = "home", Number = "" },
                    new TestPhone { Category = "mobile", Number = "5130000000" },
                    new TestPhone { Category = "work", Number = "" },
                }
            };
            List<ValidationResult> results = new List<ValidationResult>();
            var isValid = dav.TryValidateObjectRecursive(request, results, new ValidationContext(request));
            Assert.IsFalse(isValid);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("PhoneNumbers.0.Number", results[0].MemberNames.ToArray()[0]);
            Assert.AreEqual("PhoneNumbers.2.Number", results[1].MemberNames.ToArray()[0]);
        }

        [TestMethod]
        public void ObjectWithMultipleCollectionsUsesCorrectIndexPositionInResult()
        {
            var dav = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy());
            var request = new TestPerson
            {
                Name = "Test Person",
                PhoneNumbers = new[]
                {
                    new TestPhone { Category = "home", Number = "" },
                    new TestPhone { Category = "mobile", Number = "5130000000" },
                    new TestPhone { Category = "work", Number = "" },
                },
                EmailAddresses = new []
                {
                    new TestEmail  { Category = "home", Address = "" },
                    new TestEmail  { Category = "mobile", Address = "test@example.com" },
                    new TestEmail  { Category = "work", Address = "" },
                }
                
            };
            List<ValidationResult> results = new List<ValidationResult>();
            var isValid = dav.TryValidateObjectRecursive(request, results, new ValidationContext(request));
            Assert.IsFalse(isValid);
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("PhoneNumbers.0.Number", results[0].MemberNames.ToArray()[0]);
            Assert.AreEqual("PhoneNumbers.2.Number", results[1].MemberNames.ToArray()[0]);
            Assert.AreEqual("EmailAddresses.0.Address", results[2].MemberNames.ToArray()[0]);
            Assert.AreEqual("EmailAddresses.2.Address", results[3].MemberNames.ToArray()[0]);
        }

        public class TestPerson
        {
            [Required]
            public string Name { get; set; }
            public TestPhone[] PhoneNumbers { get; set; }
            public TestEmail[] EmailAddresses { get; set; }
        }

        public class TestPhone
        {
            public string Category { get; set; }
            [Required]
            public string Number { get; set; }
        }

        public class TestEmail
        {
            public string Category { get; set; }
            [Required]
            public string Address { get; set; }
        }
    }
}
