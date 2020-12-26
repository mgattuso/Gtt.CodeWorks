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
    public class DateCompareAttributeTests
    {
        [TestMethod]
        public void HappyPathBefore()
        {
            var results = new List<ValidationResult>();

            var request = new TestClass1
            {
                Start = new DateTime(2020, 1, 2),
                End = new DateTime(2020, 1, 1)
            };

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsFalse(isValid);
            Assert.AreEqual("Start must be before End", results[0].ErrorMessage);
            Assert.AreEqual(1, results[0].MemberNames.Count());
        }

        [TestMethod]
        public void NullableWhenNullAreValid()
        {
            var results = new List<ValidationResult>();

            var request = new TestClass2();

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void NullableWhenNullAreValid_Origin_Is_Null()
        {
            var results = new List<ValidationResult>();

            var request = new TestClass2
            {
                Start = new DateTime(2020,1,1)
            };

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void NullableWhenNullAreValid_Target_Is_Null()
        {
            var results = new List<ValidationResult>();

            var request = new TestClass2
            {
                End = new DateTime(2020, 1, 1)
            };

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void HappyPathDateTimeOffset()
        {
            var results = new List<ValidationResult>();

            var request = new TestClass3
            {
                Start = DateTimeOffset.Now,
                End = DateTimeOffset.Now.AddDays(-1)
            };

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsFalse(isValid);
            Assert.AreEqual("Start must be before End", results[0].ErrorMessage);
            Assert.AreEqual(1, results[0].MemberNames.Count());
        }

        [TestMethod]
        public void HappyPathDateTimeOffsetCrossTypes()
        {
            var results = new List<ValidationResult>();

            var request = new TestClass4
            {
                Start = DateTime.Today,
                End = DateTimeOffset.Now.AddDays(-1)
            };

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsFalse(isValid);
            Assert.AreEqual("Start must be before End", results[0].ErrorMessage);
            Assert.AreEqual(1, results[0].MemberNames.Count());
        }

        [TestMethod]
        public void HappyPathDateTimeOffsetCrossTypesNullable()
        {
            var results = new List<ValidationResult>();

            var request = new TestClass4a
            {
                Start = DateTime.Today,
                End = DateTimeOffset.Now.AddDays(-1)
            };

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsFalse(isValid);
            Assert.AreEqual("Start must be before End", results[0].ErrorMessage);
            Assert.AreEqual(1, results[0].MemberNames.Count());
        }

        [TestMethod]
        public void HappyPathAfter()
        {
            var results = new List<ValidationResult>();

            var request = new TestClass5
            {
                Start = DateTime.Today,
                End = DateTime.Today.AddDays(-1)
            };

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsFalse(isValid);
            Assert.AreEqual("End must be after Start", results[0].ErrorMessage);
            Assert.AreEqual(1, results[0].MemberNames.Count());
        }

        [TestMethod]
        public void HappyPathBeforeListAllProperties()
        {
            var results = new List<ValidationResult>();

            var request = new TestClass6
            {
                Start = new DateTime(2020, 1, 2),
                End = new DateTime(2020, 1, 1)
            };

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsFalse(isValid);
            Assert.AreEqual("Start must be before End", results[0].ErrorMessage);
            Assert.AreEqual(2, results[0].MemberNames.Count());
        }

        [TestMethod]
        public void HappyPathOnOrBefore()
        {
            var results = new List<ValidationResult>();

            var request = new TestClass7
            {
                Start = new DateTime(2020, 1, 1),
                End = new DateTime(2020, 1, 1)
            };

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void HappyPathOnOrAfter()
        {
            var results = new List<ValidationResult>();

            var request = new TestClass9
            {
                Start = new DateTime(2020, 1, 1),
                End = new DateTime(2020, 1, 1)
            };

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void HappyPathSame()
        {
            var results = new List<ValidationResult>();

            var request = new TestClass8
            {
                Start = new DateTime(2020, 1, 1),
                End = new DateTime(2020, 1, 1)
            };

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void HappyPathSameBefore()
        {
            var results = new List<ValidationResult>();

            var request = new TestClass8
            {
                Start = new DateTime(2020, 1, 3),
                End = new DateTime(2020, 1, 2)
            };

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void HappyPathSameAfter()
        {
            var results = new List<ValidationResult>();

            var request = new TestClass8
            {
                Start = new DateTime(2020, 1, 3),
                End = new DateTime(2020, 1, 4)
            };

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void HappyPathOnBeforeSameDayIsInvalid()
        {
            var results = new List<ValidationResult>();

            var request = new TestClass1
            {
                Start = new DateTime(2020, 1, 1),
                End = new DateTime(2020, 1, 1)
            };

            var context = new ValidationContext(request);
            var isValid = new DataAnnotationsValidator(new DottedNumberCollectionPropertyNamingStrategy()).TryValidateObjectRecursive(request, results, context);

            Assert.IsFalse(isValid);
            Assert.AreEqual("Start must be before End", results[0].ErrorMessage);
            Assert.AreEqual(1, results[0].MemberNames.Count());
        }


        public class TestClass1
        {
            [DateCompare(DateCompareCheck.Before, nameof(End))]
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }

        public class TestClass2
        {
            [DateCompare(DateCompareCheck.Before, nameof(End))]
            public DateTime? Start { get; set; }
            public DateTime? End { get; set; }
        }

        public class TestClass3
        {
            [DateCompare(DateCompareCheck.Before, nameof(End))]
            public DateTimeOffset Start { get; set; }
            public DateTimeOffset End { get; set; }
        }

        public class TestClass4
        {
            [DateCompare(DateCompareCheck.Before, nameof(End))]
            public DateTime Start { get; set; }
            public DateTimeOffset End { get; set; }
        }

        public class TestClass4a
        {
            [DateCompare(DateCompareCheck.Before, nameof(End))]
            public DateTime? Start { get; set; }
            public DateTimeOffset? End { get; set; }
        }

        public class TestClass5
        {
            public DateTime Start { get; set; }

            [DateCompare(DateCompareCheck.After, nameof(Start))]
            public DateTime End { get; set; }
        }

        public class TestClass6
        {
            [DateCompare(DateCompareCheck.Before, nameof(End), ListAllMembers = true)]
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }

        public class TestClass7
        {
            [DateCompare(DateCompareCheck.OnOrBefore, nameof(End), ListAllMembers = true)]
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }

        public class TestClass8
        {
            [DateCompare(DateCompareCheck.Same, nameof(End))]
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }

        public class TestClass9
        {
            [DateCompare(DateCompareCheck.OnOrAfter, nameof(End), ListAllMembers = true)]
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }
    }
}
