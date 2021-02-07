using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gtt.CodeWorks.Serializers.TextJson
{
    public class JsonEnumConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsEnum;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(JsonEnumConverter<>).MakeGenericType(typeToConvert),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: null,
                culture: null);

            return converter;
        }
    }
}