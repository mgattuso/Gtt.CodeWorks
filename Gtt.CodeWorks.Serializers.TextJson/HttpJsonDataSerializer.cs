using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
            var opts = TextJsonSerializerSettings.CreateJsonSerializerOptions(options, _debugMode);

            using (var stream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(stream, response, response.GetType(), opts);
                stream.Position = 0;

                using (var reader = new StreamReader(stream))
                {
                    var result = await reader.ReadToEndAsync();
                    return result;
                }
            }
        }

        public async Task<T> DeserializeRequest<T>(byte[] message, HttpDataSerializerOptions options = null) where T : BaseRequest, new()
        {
            var r = await DeserializeRequest(typeof(T), message, options);
            return r as T;
        }

        public async Task<BaseRequest> DeserializeRequest(
            Type type,
            byte[] message,
            HttpDataSerializerOptions options = null)
        {
            var opts = TextJsonSerializerSettings.CreateJsonSerializerOptions(options, _debugMode);

            if (message.Length == 0)
            {
                return (BaseRequest)Activator.CreateInstance(type);
            }

            try
            {
                using (var ms = new MemoryStream(message))
                {
                    var result = await JsonSerializer.DeserializeAsync(ms, type, opts);
                    return (BaseRequest)result;
                }
            }
            catch (JsonException ex)
            {
                var contents = Encoding.UTF8.GetString(message);

                if (ex.StackTrace.Contains("JsonConverterEnum"))
                {
                    // INVALID ENUM FOUND - TRY AND FIGURE IT OUT
                    var property = ex.Path.Substring(2, ex.Path.Length - 2);
                    throw new ValidationErrorException("Invalid enum value provided", property);
                }

                if (string.IsNullOrWhiteSpace(contents))
                {
                    return (BaseRequest)Activator.CreateInstance(type);
                }
                _logger.LogError(ex, $"Cannot deserialize request payload {contents}");
                throw new CodeWorksSerializationException($"Cannot parse payload as JSON. Payload={contents}", contents);
            }

        }

        public async Task<string> SerializeRequest(BaseRequest request, Type requestType, HttpDataSerializerOptions options = null)
        {
            var opts = TextJsonSerializerSettings.CreateJsonSerializerOptions(options, _debugMode);

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

        public async Task<T> DeserializeResponse<T>(byte[] message, HttpDataSerializerOptions options = null) where T : new()
        {
            var opts = TextJsonSerializerSettings.CreateJsonSerializerOptions(options, _debugMode);
            try
            {
                using (var ms = new MemoryStream(message))
                {
                    var result = await JsonSerializer.DeserializeAsync<T>(ms, opts);
                    return result;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError("Cannot deserialize response", ex);
                throw new CodeWorksSerializationException("Cannot deserialize JSON payload", ex);
            }
        }

        public async Task<ServiceResponse> DeserializeResponse(Type type, byte[] message, HttpDataSerializerOptions options = null)
        {
            var opts = TextJsonSerializerSettings.CreateJsonSerializerOptions(options, _debugMode);
            try
            {
                using (var ms = new MemoryStream(message))
                {
                    var result = await JsonSerializer.DeserializeAsync(ms, type, opts);
                    return (ServiceResponse)result;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError("Cannot deserialize request", ex);
                throw;
            }
        }
    }
}
