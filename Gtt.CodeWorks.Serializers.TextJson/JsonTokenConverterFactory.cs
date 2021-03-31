using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Namotion.Reflection;

namespace Gtt.CodeWorks.Serializers.TextJson
{
    public class JsonTokenConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            bool isItokenizable =  typeToConvert.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITokenizable<>));
            bool isClientToken = typeToConvert.IsSubclassOf(typeof(TokenBase));
            return isItokenizable || isClientToken;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            bool isItokenizable = typeToConvert.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITokenizable<>));
            if (isItokenizable)
            {
                JsonConverter converter = new JsonTokenConverter();
                return converter;
            }

            bool isClientToken = typeToConvert.IsSubclassOf(typeof(TokenBase));
            if (isClientToken)
            {
                return new JsonTokenBaseConverter();
            }

            throw new NotImplementedException();
        }
    }
}