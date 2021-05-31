using System;
using System.Linq;
using Namotion.Reflection;
using Newtonsoft.Json;

namespace Gtt.CodeWorks.Serializers.JsonNet
{
    public class JsonTokenConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var str = value.ToString();
            writer.WriteValue(str);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string val = reader.ReadAsString();

            if (objectType == typeof(TokenString))
            {
                return new TokenString(val);
            }

            if (objectType == typeof(TokenDate))
            {
                return new TokenDate(val);
            }

            throw new NotImplementedException($"No converter exists for {objectType.FullName}");
        }

        public override bool CanConvert(Type objectType)
        {
            bool isItokenizable = objectType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITokenizable<>));
            bool isClientToken = objectType.IsSubclassOf(typeof(TokenBase));
            return isItokenizable || isClientToken;
        }
    }
}