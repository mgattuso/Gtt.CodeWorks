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

            await using var memStream = new MemoryStream();
            await using var sw = new StreamWriter(memStream, Encoding.UTF8);
            await sw.WriteAsync("");
            memStream.Position = 0;
            var response = await serializer.DeserializeResponse<TestResponse>(sw.BaseStream);
        }

        public class TestResponse
        {

        }
    }
}
