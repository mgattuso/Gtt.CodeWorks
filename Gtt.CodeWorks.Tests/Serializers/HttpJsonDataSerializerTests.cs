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

        public class TestResponse
        {

        }
    }
}
