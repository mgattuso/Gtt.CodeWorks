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

        public async Task<T> ConvertRequest<T>(HttpRequestMessage request) where T : BaseRequest, new()
        {
            var contents = await request.Content.ReadAsStreamAsync();
            var result = await _serializer.DeserializeRequest<T>(contents);
            return result;
        }

        public async Task<HttpResponseMessage> ConvertResponse<T>(ServiceResponse<T> response) where T : new()
        {
            var serializedData = await _serializer.SerializeResponse(response);
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
