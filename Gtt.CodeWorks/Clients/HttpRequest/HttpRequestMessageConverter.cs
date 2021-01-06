using System;
using System.IO;
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
        private readonly IServiceEnvironmentResolver _environmentResolver;

        public HttpRequestMessageConverter(IHttpDataSerializer serializer, IServiceEnvironmentResolver environmentResolver)
        {
            _serializer = serializer;
            _environmentResolver = environmentResolver;
        }

        public async Task<BaseRequest> ConvertRequest(Type type, HttpRequestMessage request)
        {
            HttpDataSerializerOptions options = CreateOptionsFromHeaders(request.Headers);

            Stream contents = await request.Content.ReadAsStreamAsync();

            //TODO: REVISIT THIS SCHEMA VALIDATION CODE
            //if (ShouldValidateSchema(options))
            //{
            //    using (var ms = new MemoryStream())
            //    {
            //        using (var sr = new StreamReader(ms))
            //        {
            //            await contents.CopyToAsync(ms);
            //            ms.Seek(0, SeekOrigin.Begin);
            //            var schemaErrors = await _serializer.ValidateSchema(ms, type);

            //            if (schemaErrors.Any())
            //            {
            //                throw new SchemaValidationException($"Cannot validate payload as type of {type.FullName}", schemaErrors);
            //            }

            //            ms.Seek(0, SeekOrigin.Begin);
            //            BaseRequest validResult = await _serializer.DeserializeRequest(type, contents, options);
            //            return validResult;
            //        }
            //    }
            //}

            BaseRequest result = await _serializer.DeserializeRequest(type, contents, options);
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
            var opts = new HttpDataSerializerOptions
            {
                IncludeDependencyMetaData = ParseHeaderForEnum<IncludeDependencyMetaDataStrategy>(headers, "codeworks-prefs-dep-meta"),
                EnumSerializationMethod = ParseHeaderForEnum<EnumSerializationMethod>(headers, "codeworks-prefs-enum"),
                JsonSchemaValidation = ParseHeaderForEnum<JsonValidationStrategy>(headers, "codeworks-prefs-schema-check")
            };

            return opts;
        }

        private T ParseHeaderForEnum<T>(HttpRequestHeaders headers, string header) where T : struct
        {
            if (!headers.Contains(header))
            {
                return default(T);
            }
            var v = headers.GetValues(header).FirstOrDefault();
            return Enum.TryParse(v, true, out T t) ? t : default(T);
        }

        private bool ShouldValidateSchema(HttpDataSerializerOptions options)
        {
            if (_environmentResolver.Environment == CodeWorksEnvironment.Production)
            {
                return options.JsonSchemaValidation == JsonValidationStrategy.ForceOverride;
            }

            return options.JsonSchemaValidation == JsonValidationStrategy.Default ||
                   options.JsonSchemaValidation == JsonValidationStrategy.ForceOverride;
        }
    }
}
