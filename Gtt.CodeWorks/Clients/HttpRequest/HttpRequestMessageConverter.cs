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
        private readonly ISerializationSchema _serializationSchema;

        public HttpRequestMessageConverter(
            IHttpDataSerializer serializer, 
            IServiceEnvironmentResolver environmentResolver,
            ISerializationSchema serializationSchema)
        {
            _serializer = serializer;
            _environmentResolver = environmentResolver;
            _serializationSchema = serializationSchema;
        }

        public async Task<BaseRequest> ConvertRequest(Type type, HttpRequestMessage request)
        {
            HttpDataSerializerOptions options = CreateOptionsFromHeaders(request.Headers);

            byte[] contents = await request.Content.ReadAsByteArrayAsync();

            if (ShouldValidateSchema(options))
            {
                var schemaErrors = await _serializationSchema.ValidateSchema(contents, type, options);

                if (schemaErrors.Any())
                {
                    throw new SchemaValidationException($"Cannot validate payload as type of {type.FullName}", schemaErrors);
                }

                BaseRequest validResult = await _serializer.DeserializeRequest(type, contents, options);
                return validResult;
            }

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
                var prodValidationOptions = new[]
                {
                    JsonValidationStrategy.ForceStrict,
                    JsonValidationStrategy.ForceAllowAdditionalProperties,
                };
                return prodValidationOptions.Contains(options.JsonSchemaValidation);
            }

            var nonProdValidationOptions = new[]
            {
                JsonValidationStrategy.DefaultStrict,
                JsonValidationStrategy.DefaultAllowAdditionalProperties,
                JsonValidationStrategy.ForceStrict,
                JsonValidationStrategy.ForceAllowAdditionalProperties,
            };

            return nonProdValidationOptions.Contains(options.JsonSchemaValidation);
        }
    }
}
