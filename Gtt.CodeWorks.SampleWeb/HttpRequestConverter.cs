using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Gtt.CodeWorks.AspNet
{
    public class HttpRequestConverter
    {
        private readonly IHttpDataSerializer _serializer;

        public HttpRequestConverter(IHttpDataSerializer serializer)
        {
            _serializer = serializer;
        }

        public async Task<T> ConvertRequest<T>(HttpRequest request) where T : BaseRequest, new()
        {
            //TODO: CALL THE TYPE BASED VERSION?
            if (request.ContentLength.GetValueOrDefault() > 0)
            {
                var contents = request.Body;
                var result = await _serializer.DeserializeRequest<T>(contents);
                return result;
            }

            return new T();
        }

        public async Task<BaseRequest> ConvertRequest(Type requestType, HttpRequest request)
        {
            if (request.ContentLength.GetValueOrDefault() > 0)
            {
                var contents = request.Body;
                var result = await _serializer.DeserializeRequest(requestType, contents);
                return result;
            }

            return (BaseRequest)Activator.CreateInstance(requestType);
        }

        public async Task ConvertResponse<T>(ServiceResponse<T> response, HttpResponse httpResponse) where T : new()
        {
            var serializedData = await _serializer.SerializeResponse(response);
            var contentType = _serializer.ContentType;
            var encoding = _serializer.Encoding;

            httpResponse.StatusCode = response.MetaData.Result.HttpStatusCode();
            httpResponse.ContentType = contentType;

            await httpResponse.WriteAsync(serializedData, encoding);
        }

        public async Task ConvertResponse(ServiceResponse response, HttpResponse httpResponse)
        {
            var serializedData = await _serializer.SerializeResponse(response);
            var contentType = _serializer.ContentType;
            var encoding = _serializer.Encoding;

            httpResponse.StatusCode = response.MetaData.Result.HttpStatusCode();
            httpResponse.ContentType = contentType;

            await httpResponse.WriteAsync(serializedData, encoding);
        }
    }
}