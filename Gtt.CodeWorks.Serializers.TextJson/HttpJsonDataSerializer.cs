using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Validation;

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
                await JsonSerializer.SerializeAsync(stream, response, response.GetType(), opts);
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

        public async Task<BaseRequest> DeserializeRequest(Type type, Stream message,
            HttpDataSerializerOptions options = null)
        {
            var opts = CreateJsonSerializerOptions(options);

            using (var ms = new MemoryStream())
            {
                using (var reader = new StreamReader(ms))
                {
                    await message.CopyToAsync(ms);

                    if (ms.Length == 0)
                    {
                        return (BaseRequest)Activator.CreateInstance(type);
                    }

                    ms.Seek(0, SeekOrigin.Begin);

                    try
                    {
                        var result = await JsonSerializer.DeserializeAsync(ms, type, opts);
                        return (BaseRequest)result;
                    }
                    catch (JsonException ex)
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        var contents = await reader.ReadToEndAsync();
                        if (string.IsNullOrWhiteSpace(contents))
                        {
                            return (BaseRequest)Activator.CreateInstance(type);
                        }
                        _logger.LogError(ex, $"Cannot deserialize request payload {contents}");
                        throw new CodeWorksSerializationException($"Cannot parse payload as JSON. Payload={contents}", contents);
                    }
                }
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

        public async Task<IDictionary<string, string[]>> ValidateSchema(Stream stream, Type type)
        {
            var sr = new StreamReader(stream);

            var content = await sr.ReadToEndAsync();
            JsonSchema schema = JsonSchema.FromType(type, new JsonSchemaGeneratorSettings
            {
                ExcludedTypeNames = new[] { "CorrelationId", "ServiceHop", "SessionId" }
            });

            ICollection<ValidationError> errors = schema.Validate(content);

            var dict = new Dictionary<string, string[]>();

            foreach (var err in errors)
            {
                RecursivelyGetErrors(dict, err, schema);
            }

            var d = schema.ToJson();
            Console.WriteLine(d);

            if (dict.Any())
            {
                dict.AddOrAppendValue("schema", schema.ToJson());
            }

            return dict;

        }

        private void RecursivelyGetErrors(Dictionary<string, string[]> dict, ValidationError err, JsonSchema schema)
        {
            Console.WriteLine(err.Path);
            if (err is ChildSchemaValidationError child)
            {
                foreach (var childError in child.Errors)
                {
                    foreach (var ce in childError.Value)
                    {
                        RecursivelyGetErrors(dict, ce, childError.Key);
                    }
                }
            }

            var kind = err.Kind;
            string property = err.Property;
            string path = err.Path;
            Console.WriteLine($"{kind} {property} {path}");
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
