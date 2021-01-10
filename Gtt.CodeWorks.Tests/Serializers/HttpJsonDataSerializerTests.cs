using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EasyNetQ.Logging;
using Gtt.CodeWorks.Serializers.TextJson;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Gtt.CodeWorks.Tests.Serializers
{

    [TestClass]
    public class HttpJsonDataSerializerTests
    {

        [TestMethod, ExpectedException(typeof(CodeWorksSerializationException))]
        public async Task DeserializeBlankString()
        {
            // ARRANGE
            var logger = Mock.Of<ILogger<HttpJsonDataSerializer>>();
            var serializer = new HttpJsonDataSerializer(logger);

            string contents = "";
            byte[] message = Encoding.UTF8.GetBytes(contents);
            var response = await serializer.DeserializeResponse<TestResponse>(message);
        }

        [TestMethod]
        public void SerializeError()
        {
            // ARRANGE
            var logger = Mock.Of<ILogger<HttpJsonDataSerializer>>();
            var serializer = new HttpJsonDataSerializer(logger);

            var errors = new Dictionary<string, object>();
            errors.AddOrAppendValue("a",new { a = new[] {1, 2} }, forceArray:false);
            errors.AddOrAppendValue("a", new[] { 3, 4 }, forceArray: false);
            var md = new ResponseMetaData("TestService", DateTimeOffset.UtcNow, 1, Guid.Empty, ServiceResult.PermanentError, errors);
            var re = new ServiceResponse<TestResponse>(null, md);
            var str = serializer.SerializeResponse(re, re.GetType());
            Assert.IsNotNull(str);
        }

        [TestMethod, ExpectedException(typeof(ValidationErrorException))]
        public async Task SerializeInvalidEnumReturnsValidationError()
        {
            // ARRANGE
            var logger = Mock.Of<ILogger<HttpJsonDataSerializer>>();
            var serializer = new HttpJsonDataSerializer(logger);

            var str = @"{ ""Category"":""Wrong"" }";

            var result = await serializer.DeserializeRequest<TestRequest>(Encoding.UTF8.GetBytes(str));
        }

        public class TestResponse
        {

        }

        public enum TestEnum
        {
            Value1,
            Value2
        }

        public class TestRequest : BaseRequest
        {
            public TestEnum Category { get; set; }
        }
    }
}
