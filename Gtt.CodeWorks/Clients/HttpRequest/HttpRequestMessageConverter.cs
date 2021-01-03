using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Clients.HttpRequest
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
            var options = CreateOptionsFromHeaders(request.Headers);
            var contents = await request.Content.ReadAsStreamAsync();
            var result = await _serializer.DeserializeRequest(type, contents, options);
            return result;
        }

        public async Task<HttpResponseMessage> ConvertResponse(HttpRequestMessage request, ServiceResponse response, Type type)
        {
            var options = CreateOptionsFromHeaders(request.Headers);
            var serializedData = await _serializer.SerializeResponse(response, type, options);
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

        private HttpDataSerializerOptions CreateOptionsFromHeaders(HttpRequestHeaders headers)
        {
            var opts = new HttpDataSerializerOptions();
            if (headers.Contains("codeworks-prefs-enum"))
            {
                var val = headers.GetValues("codeworks-prefs-enum").FirstOrDefault() ?? "";
                if (Equals(val, "numeric"))
                {
                    opts.EnumSerializationMethod = EnumSerializationMethod.Numeric;
                }
                if (Equals(val, "string"))
                {
                    opts.EnumSerializationMethod = EnumSerializationMethod.String;
                }
                if (Equals(val, "object"))
                {
                    opts.EnumSerializationMethod = EnumSerializationMethod.Object;
                }
            }

            string includeDependencyMetaDataHeader = "codeworks-prefs-dep-meta";

            if (headers.Contains(includeDependencyMetaDataHeader))
            {
                var val = headers.GetValues(includeDependencyMetaDataHeader).FirstOrDefault() ?? "";
                if (Equals(val, "full"))
                {
                    opts.IncludeDependencyMetaData = true;
                }
            }

            return opts;
        }
    }
}
