using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Gtt.CodeWorks.Serializers.TextJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.Serializers
{
    [TestClass]
    public class JsonLogObjectSerializerTests
    {
        [TestMethod]
        public async Task Serialize_Happy_Path()
        {
            // ARRANGE
            var obj = new TestA()
            {
                Message = "Hello World"
            };
            var serializer = new JsonLogObjectSerializer();

            // ACT
            var result = await serializer.Serialize(obj);

            // ASSERT
            Assert.AreEqual("{\"message\":\"Hello World\"}", result);
        }

        [TestMethod]
        public async Task Serialize_Enums_As_Text()
        {
            // ARRANGE
            var obj = new TestB()
            {
                Enum = TestB.TestBEnum.Category1
            };
            var serializer = new JsonLogObjectSerializer();

            // ACT
            var result = await serializer.Serialize(obj);

            // ASSERT
            Assert.AreEqual("{\"enum\":\"Category1\"}", result);
        }

        [TestMethod]
        public async Task Serialize_Null_Returns_The_String_Null()
        {
            // arrange
            object val = null;
            var serializer = new JsonLogObjectSerializer();

            // ACT
            var result = await serializer.Serialize(val);

            // ASSERT
            Assert.AreEqual("null", result);
        }

        [TestMethod]
        public async Task Serialize_String_Literal_Returns_The_Quoted_String()
        {
            // arrange
            string val = "Hello World";
            var serializer = new JsonLogObjectSerializer();

            // ACT
            var result = await serializer.Serialize(val);

            // ASSERT
            Assert.AreEqual("\"Hello World\"", result);
        }

        [TestMethod]
        public async Task Serialize_Numeric_Returns_The_Number()
        {
            // arrange
            int val = 99;
            var serializer = new JsonLogObjectSerializer();

            // ACT
            var result = await serializer.Serialize(val);

            // ASSERT
            Assert.AreEqual("99", result);
        }

        public class TestA
        {
            public string Message { get; set; }
        }

        public class TestB
        {
            public TestBEnum Enum { get; set; }
            public enum TestBEnum
            {
                Category1,
                Category2
            }
        }
    }
}
