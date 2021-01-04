using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Gtt.CodeWorks.Serializers.TextJson
{
    public class HttpJsonDataSerializer : IHttpDataSerializer
    {
        private readonly ILogger<HttpJsonDataSerializer> _logger;
        private readonly bool _debugMode;

        public HttpJsonDataSerializer(ILogger<HttpJsonDataSerializer> logger)
        {
            _logger = logger;
            _debugMode = true;
        }

        public string ContentType => "application/json";
        public Encoding Encoding => Encoding.UTF8;
        public async Task<string> SerializeResponse(ServiceResponse response, Type responseType, HttpDataSerializerOptions options = null)
        {
            var opts = CreateJsonSerializerOptions(options);

            using (var stream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(stream, response, responseType, opts);
                stream.Position = 0;

                using (var reader = new StreamReader(stream))
                {
                    var result = await reader.ReadToEndAsync();
                    return result;
                }
            }
        }

        public async Task<T> DeserializeRequest<T>(Stream stream, HttpDataSerializerOptions options = null) where T : BaseRequest, new()
        {
            var opts = CreateJsonSerializerOptions(options);

            try
            {
                var result = await JsonSerializer.DeserializeAsync<T>(stream, opts);
                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError("Cannot deserialize request", ex);
                throw;
            }
        }

        public async Task<BaseRequest> DeserializeRequest(Type type, Stream message, HttpDataSerializerOptions options = null)
        {
            var opts = CreateJsonSerializerOptions(options);

            try
            {
                var result = await JsonSerializer.DeserializeAsync(message, type, opts);
                return (BaseRequest)result;
            }
            catch (JsonException ex)
            {
                _logger.LogError("Cannot deserialize request", ex);
                throw;
            }
        }

        public async Task<string> SerializeRequest(BaseRequest request, Type requestType, HttpDataSerializerOptions options = null)
        {
            var opts = CreateJsonSerializerOptions(options);

            using (var stream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(stream, request, requestType, opts);
                stream.Position = 0;

                using (var reader = new StreamReader(stream))
                {
                    var result = await reader.ReadToEndAsync();
                    return result;
                }
            }

            

            
        }

        public async Task<T> DeserializeResponse<T>(Stream stream, HttpDataSerializerOptions options = null) where T : new()
        {
            var opts = CreateJsonSerializerOptions(options);
            try
            {
                var result = await JsonSerializer.DeserializeAsync<T>(stream, opts);
                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError("Cannot deserialize response", ex);
                throw new CodeWorksSerializationException("Cannot deserialize JSON payload", ex);
            }
        }

        public async Task<ServiceResponse> DeserializeResponse(Type type, Stream message, HttpDataSerializerOptions options = null)
        {
            var opts = CreateJsonSerializerOptions(options);
            try
            {
                var result = await JsonSerializer.DeserializeAsync(message, type, opts);
                return (ServiceResponse)result;
            }
            catch (JsonException ex)
            {
                _logger.LogError("Cannot deserialize request", ex);
                throw;
            }
        }

        private JsonSerializerOptions CreateJsonSerializerOptions(HttpDataSerializerOptions options)
        {
            options = options ?? new HttpDataSerializerOptions();
            var opts = new JsonSerializerOptions
            {
                WriteIndented = _debugMode,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true
            };

            if (options.EnumSerializationMethod == EnumSerializationMethod.Object)
                opts.Converters.Add(new JsonEnumConverter());
            if (options.EnumSerializationMethod == EnumSerializationMethod.String)
                opts.Converters.Add(new JsonStringEnumConverter());

            return opts;
        }
    }
}
