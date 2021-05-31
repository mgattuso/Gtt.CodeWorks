using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Gtt.CodeWorks.Serializers.JsonNet
{
    public class HttpJsonNetDataSerializer : IHttpDataSerializer
    {
        private readonly ILogger<HttpJsonNetDataSerializer> _logger;
        private readonly bool _debugMode;

        public HttpJsonNetDataSerializer(ILogger<HttpJsonNetDataSerializer> logger)
        {
            _logger = logger;
            _debugMode = true;
        }

        public string ContentType => "application/json";
        public Encoding Encoding => Encoding.UTF8;
        public Task<string> SerializeResponse(ServiceResponse response, Type responseType, HttpDataSerializerOptions options = null)
        {
            var settings = JsonNetSerializerSettings.CreateJsonSerializerSettings(options, _debugMode);
            var res = JsonConvert.SerializeObject(response, settings);
            return Task.FromResult(res);
        }

        public async Task<T> DeserializeRequest<T>(byte[] message, HttpDataSerializerOptions options = null) where T : BaseRequest, new()
        {
            var r = await DeserializeRequest(typeof(T), message, options);
            return r as T;
        }

        public async Task<BaseRequest> DeserializeRequest(Type type, byte[] message, HttpDataSerializerOptions options = null)
        {
            var settings = JsonNetSerializerSettings.CreateJsonSerializerSettings(options, _debugMode);

            if (message.Length == 0)
            {
                return (BaseRequest)Activator.CreateInstance(type);
            }

            try
            {
                await using var ms = new MemoryStream(message);
                using var reader = new StreamReader(ms, Encoding.UTF8);
                var result = JsonSerializer.Create(settings).Deserialize(reader, type);
                return (BaseRequest) result;
            }
            catch (JsonSerializationException ex)
            {
                var contents = Encoding.UTF8.GetString(message);
                if (ex.StackTrace.Contains("StringEnumConverter"))
                {
                    // INVALID ENUM FOUND - TRY AND FIGURE IT OUT
                    var property = ex.Path;
                    throw new ValidationErrorException("Invalid value provided", property);
                }

                throw;
            }
            catch (JsonException ex)
            {
                var t = ex.GetType();

                var contents = Encoding.UTF8.GetString(message);
                //StringEnumConverter

                if (ex.StackTrace.Contains("StringEnumConverter"))
                {
                    // INVALID ENUM FOUND - TRY AND FIGURE IT OUT
                    var property = "";
                    throw new ValidationErrorException("Invalid enum value provided", property);
                }

                if (string.IsNullOrWhiteSpace(contents))
                {
                    return (BaseRequest) Activator.CreateInstance(type);
                }

                _logger.LogError(ex, $"Cannot deserialize request payload {contents}");
                throw new CodeWorksSerializationException($"Cannot parse payload as JSON. Payload={contents}",
                    contents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cannot deserialize request payload");
                throw new CodeWorksSerializationException($"Cannot parse payload as JSON");
            }
        }

        public async Task<string> SerializeRequest(BaseRequest request, Type requestType, HttpDataSerializerOptions options = null)
        {
            var settings = JsonNetSerializerSettings.CreateJsonSerializerSettings(options, _debugMode);
            string contents = "";
            await using var stream = new MemoryStream();
            await using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                var serializer = JsonSerializer.Create(settings);
                serializer.Serialize(writer, request, requestType);
            }

            stream.Position = 0;
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                contents = await reader.ReadToEndAsync();
            }

            return contents;
        }


        public async Task<T> DeserializeResponse<T>(byte[] message, HttpDataSerializerOptions options = null) where T : new()
        {
            var settings = JsonNetSerializerSettings.CreateJsonSerializerSettings(options, _debugMode);

            if (message.Length == 0)
            {
                throw new CodeWorksSerializationException("Cannot deserialize an empty payload");
            }

            try
            {
                await using var ms = new MemoryStream(message);
                using var reader = new StreamReader(ms, Encoding.UTF8);
                var result = JsonSerializer.Create(settings).Deserialize(reader, typeof(T));
                return (T)result;
            }
            catch (JsonException ex)
            {
                var contents = Encoding.UTF8.GetString(message);

                if (string.IsNullOrWhiteSpace(contents))
                {
                    return new T();
                }
                _logger.LogError(ex, $"Cannot deserialize request payload {contents}");
                throw new CodeWorksSerializationException($"Cannot parse payload as JSON. Payload={contents}", contents);
            }
        }

        public async Task<ServiceResponse> DeserializeResponse(Type type, byte[] message, HttpDataSerializerOptions options = null)
        {
            var settings = JsonNetSerializerSettings.CreateJsonSerializerSettings(options, _debugMode);

            if (message.Length == 0)
            {
                return (ServiceResponse)Activator.CreateInstance(type);
            }

            try
            {
                await using var ms = new MemoryStream(message);
                using var reader = new StreamReader(ms, Encoding.UTF8);
                var result = JsonSerializer.Create(settings).Deserialize(reader, type);
                return (ServiceResponse)result;
            }
            catch (JsonException ex)
            {
                var contents = Encoding.UTF8.GetString(message);

                if (string.IsNullOrWhiteSpace(contents))
                {
                    return (ServiceResponse)Activator.CreateInstance(type);
                }
                _logger.LogError(ex, $"Cannot deserialize request payload {contents}");
                throw new CodeWorksSerializationException($"Cannot parse payload as JSON. Payload={contents}", contents);
            }
        }
    }
}
