using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Web
{
    public class HttpRequestMessageConverter
    {
        private readonly IHttpDataSerializer _serializer;

        public HttpRequestMessageConverter(IHttpDataSerializer serializer)
        {
            _serializer = serializer;
        }

        public async Task<BaseRequest> ConvertRequest(Type type, HttpRequestMessage request)
        {
            var contents = await request.Content.ReadAsStreamAsync();
            var result = await _serializer.DeserializeRequest(type, contents);
            return result;
        }

        public async Task<HttpResponseMessage> ConvertResponse(ServiceResponse response, Type type)
        {
            var serializedData = await _serializer.SerializeResponse(response, type);
            var contentType = _serializer.ContentType;
            var encoding = _serializer.Encoding;
            var httpMsg = new HttpResponseMessage
            {
                StatusCode = (HttpStatusCode)response.MetaData.Result.HttpStatusCode(),
                Content = new StringContent(serializedData, encoding, contentType)
            };
            httpMsg.Headers.Add(nameof(response.MetaData.CorrelationId), $"{response.MetaData.CorrelationId}");

            return httpMsg;
        }
    }
}
