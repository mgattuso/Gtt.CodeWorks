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
        public async Task<string> SerializeResponse(ServiceResponse response, Type responseType)
        {
            var opts = new JsonSerializerOptions
            {
                WriteIndented = _debugMode,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true
            };
            opts.Converters.Add(new JsonStringEnumConverter());

            await using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, response, responseType, opts);

            stream.Position = 0;
            using var reader = new StreamReader(stream);
            var result = await reader.ReadToEndAsync();
            return result;
        }

        public async Task<T> DeserializeRequest<T>(Stream stream) where T : BaseRequest, new()
        {
            var opts = new JsonSerializerOptions
            {
                IgnoreNullValues = false,
                PropertyNameCaseInsensitive = true,
            };

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

        public async Task<BaseRequest> DeserializeRequest(Type type, Stream message)
        {
            var opts = new JsonSerializerOptions
            {
                IgnoreNullValues = false,
                PropertyNameCaseInsensitive = true,
            };

            opts.Converters.Add(new JsonStringEnumConverter());

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

        public async Task<string> SerializeRequest(BaseRequest request, Type requestType)
        {
            var opts = new JsonSerializerOptions
            {
                WriteIndented = _debugMode,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true
            };
            opts.Converters.Add(new JsonStringEnumConverter());

            await using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, request, requestType, opts);

            stream.Position = 0;
            using var reader = new StreamReader(stream);
            var result = await reader.ReadToEndAsync();
            return result;
        }

        public async Task<T> DeserializeResponse<T>(Stream stream) where T : new()
        {
            var opts = new JsonSerializerOptions
            {
                IgnoreNullValues = false,
                PropertyNameCaseInsensitive = true
            };
            opts.Converters.Add(new JsonStringEnumConverter());

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

        public async Task<ServiceResponse> DeserializeResponse(Type type, Stream message)
        {
            var opts = new JsonSerializerOptions
            {
                IgnoreNullValues = false,
                PropertyNameCaseInsensitive = true,
            };

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
    }
}
