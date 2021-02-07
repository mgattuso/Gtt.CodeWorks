using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gtt.CodeWorks.Serializers.TextJson
{
    public class JsonTokenConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITokenizable<>));
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            JsonConverter converter = new JsonTokenConverter();
            return converter;
        }
    }
}