using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Namotion.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Validation;
using NJsonSchema.Validation.FormatValidators;
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

        public async Task<T> DeserializeRequest<T>(byte[] message, HttpDataSerializerOptions options = null) where T : BaseRequest, new()
        {
            var opts = CreateJsonSerializerOptions(options);

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
                _logger.LogError("Cannot deserialize request", ex);
                throw;
            }
        }

        public async Task<BaseRequest> DeserializeRequest(
            Type type,
            byte[] message,
            HttpDataSerializerOptions options = null)
        {
            var opts = CreateJsonSerializerOptions(options);

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

        public async Task<T> DeserializeResponse<T>(byte[] message, HttpDataSerializerOptions options = null) where T : new()
        {
            var opts = CreateJsonSerializerOptions(options);
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

        public Task<IDictionary<string, object>> ValidateSchema(byte[] message, Type type, HttpDataSerializerOptions options = null)
        {
            options = options ?? new HttpDataSerializerOptions();
            string contents = Encoding.UTF8.GetString(message);

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new StringEnumConverter());
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            JsonSchema schema = JsonSchema.FromType(type, new JsonSchemaGeneratorSettings
            {
                FlattenInheritanceHierarchy = true,
                AlwaysAllowAdditionalObjectProperties = AllowAdditionalPropertiesForJsonSchemaValidation(options),
                GenerateEnumMappingDescription = true,
                ExcludedTypeNames = new [] { "ServiceHop" },
                SerializerSettings = settings,
                ReflectionService = new CustomReflectionService() 
            });
            ICollection<ValidationError> errors = schema.Validate(contents, new EnumFormatValidator());
            IDictionary<string, object> dict = new Dictionary<string, object>();
            foreach (var err in errors)
            {
                RecursivelyGetErrors(dict, err, schema);
            }

            if (dict.Any())
            {
                dict.AddOrAppendValue("errorType", "jsonSchemaValidation", forceArray: false);
                dict.AddOrAppendValue("schema", schema.ToJson(Formatting.None));
                _logger.LogTrace(schema.ToJson());
            }

            return Task.FromResult(dict);

        }

        private void RecursivelyGetErrors(IDictionary<string, object> dict, ValidationError err, JsonSchema schema)
        {
            if (err is ChildSchemaValidationError child)
            {
                foreach (var childError in child.Errors)
                {
                    foreach (var ce in childError.Value)
                    {
                        dict.AddOrAppendValue($"{err.Path}.{ce.Property}", ce.Kind.ToString());
                        RecursivelyGetErrors(dict, ce, childError.Key);
                    }
                }
            }

            dict.AddOrAppendValue($"{err.Property}", err.Kind.ToString());
        }

        public async Task<ServiceResponse> DeserializeResponse(Type type, byte[] message, HttpDataSerializerOptions options = null)
        {
            var opts = CreateJsonSerializerOptions(options);
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

        private bool AllowAdditionalPropertiesForJsonSchemaValidation(HttpDataSerializerOptions options)
        {
            var allowProps = new[]
            {
                JsonValidationStrategy.DefaultAllowAdditionalProperties,
                JsonValidationStrategy.ForceAllowAdditionalProperties
            };

            return allowProps.Contains(options.JsonSchemaValidation);
        }
    }

    public class EnumFormatValidator : IFormatValidator
    {
        public bool IsValid(string value, JTokenType tokenType)
        {
            return true;
        }

        public ValidationErrorKind ValidationErrorKind { get; set; }
        public string Format => "Enum";
    }

    public class CustomReflectionService : DefaultReflectionService
    {
        public override JsonTypeDescription GetDescription(ContextualType contextualType,
            ReferenceTypeNullHandling defaultReferenceTypeNullHandling, JsonSchemaGeneratorSettings settings)
        {
            return base.GetDescription(contextualType, defaultReferenceTypeNullHandling, settings);
        }


        public override bool IsNullable(ContextualType contextualType, ReferenceTypeNullHandling defaultReferenceTypeNullHandling)
        {
            return base.IsNullable(contextualType, defaultReferenceTypeNullHandling);
        }

        public override bool IsStringEnum(ContextualType contextualType, JsonSerializerSettings serializerSettings)
        {
            return base.IsStringEnum(contextualType, serializerSettings);
        }
    }
}
