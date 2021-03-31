using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Generation.TypeMappers;
using NJsonSchema.Validation;

namespace Gtt.CodeWorks.Serializers.TextJson
{
    public class JsonSerializationSchema : ISerializationSchema
    {
        private readonly ILogger<JsonSerializationSchema> _logger;

        public JsonSerializationSchema(ILogger<JsonSerializationSchema> logger)
        {
            _logger = logger;
        }

        public JsonSchema GetSchema(Type t, HttpDataSerializerOptions options, bool useStringEnums, bool requireReferenceTypes)
        {
            var settings = GetSchemaSerializationSettings(useStringEnums);
            JsonSchema schema = JsonSchema.FromType(t, new JsonSchemaGeneratorSettings
            {
                FlattenInheritanceHierarchy = true,
                AlwaysAllowAdditionalObjectProperties = AllowAdditionalPropertiesForJsonSchemaValidation(options),
                GenerateEnumMappingDescription = true,
                DefaultReferenceTypeNullHandling = requireReferenceTypes ? ReferenceTypeNullHandling.NotNull : ReferenceTypeNullHandling.Null,
                SerializerSettings = settings,
                ReflectionService = new CustomReflectionService(),
                TypeMappers = new List<ITypeMapper>
                {
                    new TokenStringTypeMapper(),
                    new TokenDateTypeMapper(),
                    new ClientTokenStringTypeMapper(),
                    new ClientTokenDateTypeMapper()
                }
            });
            
            return schema;
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

        private static void RecursivelyGetErrors(IDictionary<string, string[]> dict, ValidationError err, JsonSchema schema)
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

        private JsonSerializerSettings GetSchemaSerializationSettings(bool useStringEnums)
        {
            var settings = new JsonSerializerSettings();
            if (useStringEnums)
            {
                settings.Converters.Add(new StringEnumConverter());
            }

            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            return settings;
        }


        public Task<IDictionary<string, string[]>> ValidateSchema(byte[] message, Type type, HttpDataSerializerOptions options = null)
        {
            options = options ?? new HttpDataSerializerOptions();
            string contents = Encoding.UTF8.GetString(message);

            var schema = GetSchema(
                type, 
                options, 
                options.EnumSerializationMethod == EnumSerializationMethod.String, requireReferenceTypes: 
                false);

            ICollection<ValidationError> errors = schema.Validate(contents, new EnumFormatValidator());
            IDictionary<string, string[]> dict = new Dictionary<string, string[]>();
            foreach (var err in errors)
            {
                RecursivelyGetErrors(dict, err, schema);
            }

            if (dict.Any())
            {
                dict.AddOrAppendValue("errorType", "jsonSchemaValidation");
                dict.AddOrAppendValue("schema", schema.ToJson(Formatting.None));
                _logger.LogTrace(schema.ToJson());
            }

            return Task.FromResult(dict);

        }
        public Task<string> SerializeSchema(Type t, HttpDataSerializerOptions options = null)
        {
            options = options ?? new HttpDataSerializerOptions();
            var schema = GetSchema(t, options, useStringEnums: false, requireReferenceTypes: false);
            return Task.FromResult(schema.ToJson(Formatting.Indented));
        }

        public async Task<string> SerializeErrorReport(IEnumerable<ErrorCodeData> errors)
        {
            var options = TextJsonSerializerSettings.CreateJsonSerializerOptions(new HttpDataSerializerOptions(), true);
            using (var stream = new MemoryStream())
            {
                await System.Text.Json.JsonSerializer.SerializeAsync(stream, errors, options);
                stream.Position = 0;

                using (var reader = new StreamReader(stream))
                {
                    var result = await reader.ReadToEndAsync();
                    return result;
                }
            }
        }

        public Task<string> SerializeExample(Type t, HttpDataSerializerOptions options = null)
        {
            options = options ?? new HttpDataSerializerOptions();
            var schema = GetSchema(t, options, useStringEnums: true, requireReferenceTypes: true);
            var sample = schema.ToSampleJson();
            var json = sample.ToString(Formatting.Indented);
            return Task.FromResult(json);
        }
    }

    public class TokenStringTypeMapper : ITypeMapper
    {
        public void GenerateSchema(JsonSchema schema, TypeMapperContext context)
        {
            schema.Type = JsonObjectType.String;
            schema.Description = "TokenString. Accepts a string but will be converted to tokenize string structure";
            schema.Format = "TokenString";
        }

        public Type MappedType => typeof(TokenString);
        public bool UseReference => false;
    }

    public class ClientTokenStringTypeMapper : ITypeMapper
    {
        public void GenerateSchema(JsonSchema schema, TypeMapperContext context)
        {
            schema.Type = JsonObjectType.String;
            schema.Description = "ClientTokenString. Accepts a string but will be converted to tokenize string structure";
            schema.Format = "ClientTokenString";
        }

        public Type MappedType => typeof(ClientTokenString);
        public bool UseReference => false;
    }

    public class TokenDateTypeMapper : ITypeMapper
    {
        public void GenerateSchema(JsonSchema schema, TypeMapperContext context)
        {
            schema.Type = JsonObjectType.String;
            schema.Description = "TokenDate. Accepts a date formatted date but will be converted to tokenize string structure";
            schema.Format = "TokenDate";
        }

        public Type MappedType => typeof(TokenDate);
        public bool UseReference => false;
    }

    public class ClientTokenDateTypeMapper : ITypeMapper
    {
        public void GenerateSchema(JsonSchema schema, TypeMapperContext context)
        {
            schema.Type = JsonObjectType.String;
            schema.Description = "ClientTokenDate. Accepts a date formatted date but will be converted to tokenize string structure";
            schema.Format = "ClientTokenDate";
        }

        public Type MappedType => typeof(ClientTokenDate);
        public bool UseReference => false;
    }
}
