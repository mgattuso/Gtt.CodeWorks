using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

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
            T result = (T)null;
            //TODO: CALL THE TYPE BASED VERSION?
            if (request.ContentLength.GetValueOrDefault() > 0)
            {
                var contents = request.Body;
                result = await _serializer.DeserializeRequest<T>(contents);
            }

            if (request.Query.Any())
            {
                result ??= new T();
                PropertyInfo[] properties = typeof(T).GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    var qv = request.Query[property.Name];
                    var val = Convert.ChangeType("", property.PropertyType);
                    property.SetValue(result, val);
                }
            }

            return result ?? new T();
        }

        public async Task<BaseRequest> ConvertRequest(Type requestType, HttpRequest request, HttpDataSerializerOptions options)
        {
            options ??= new HttpDataSerializerOptions();
            BaseRequest result = null;
            if (request.ContentLength.GetValueOrDefault() > 0)
            {
                var contents = request.Body;
                result = await _serializer.DeserializeRequest(requestType, contents, options);
            }

            if (request.Query.Any())
            {
                result ??= (BaseRequest)Activator.CreateInstance(requestType);
                PropertyInfo[] properties = requestType.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    StringValues qv = request.Query[property.Name];
                    if (!string.IsNullOrWhiteSpace(qv))
                    {
                        try
                        {
                            var val = GetFromString(qv, property.PropertyType);
                            property.SetValue(result, val);
                        }
                        catch (Exception)
                        {
                            throw new ValidationErrorException($"Cannot convert {qv} to {property.PropertyType.Name}", property.Name);
                        }
                    }
                }
            }

            return result ?? (BaseRequest)Activator.CreateInstance(requestType);
        }

        public async Task ConvertResponse<T>(ServiceResponse<T> response, HttpResponse httpResponse, HttpDataSerializerOptions options) where T : new()
        {
            var serializedData = await _serializer.SerializeResponse(response, typeof(ServiceResponse<T>), options);
            var contentType = _serializer.ContentType;
            var encoding = _serializer.Encoding;

            httpResponse.StatusCode = response.MetaData.Result.HttpStatusCode();
            httpResponse.ContentType = contentType;

            await httpResponse.WriteAsync(serializedData, encoding);
        }

        public async Task ConvertResponse(ServiceResponse response, Type responseType, HttpResponse httpResponse, HttpDataSerializerOptions options = null)
        {
            options ??= new HttpDataSerializerOptions();
            var serializedData = await _serializer.SerializeResponse(response, responseType, options);
            var contentType = _serializer.ContentType;
            var encoding = _serializer.Encoding;

            httpResponse.StatusCode = response.MetaData.Result.HttpStatusCode();
            httpResponse.ContentType = contentType;

            await httpResponse.WriteAsync(serializedData, encoding);
        }

        public static object GetFromString(string str, Type type)
        {
            //https://stackoverflow.com/a/8626476/185045
            var foo = TypeDescriptor.GetConverter(type);
            return foo.ConvertFromInvariantString(str);
        }
    }
}