using System.Text;
using System.Threading.Tasks;
using Gtt.CodeWorks.Serializers.TextJson;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Gtt.CodeWorks.Tests.Serializers.TextJsonSerializers
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

        [TestMethod, ExpectedException(typeof(CodeWorksSerializationException))]
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
