using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Newtonsoft.Json;

namespace Gtt.CodeWorks.Serializers.JsonNet
{
    public class JsonEnumObjectConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null) return;

            var v = Convert.ToInt32(value);
            var description = ((Enum)value).ToDescription();

            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteValue(value.ToString());
            writer.WritePropertyName("value");
            writer.WriteValue(v);
            writer.WritePropertyName("description");
            writer.WriteValue(description);
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                if (reader.TokenType == JsonToken.Integer)
                {
                    var intV = reader.ReadAsInt32();
                    if (Enum.IsDefined(objectType, intV))
                    {
                        return Enum.ToObject(objectType, intV);
                    }
                    throw new ValidationErrorException($"Cannot create enum of type{objectType.Name} from the value {intV}", "");
                }

                if (reader.TokenType == JsonToken.StartObject)
                {
                    var nextToken = reader.TokenType;
                    var name = "";
                    int? value = null;
                    while (nextToken != JsonToken.EndObject)
                    {
                        reader.Read();
                        nextToken = reader.TokenType;
                        if (nextToken == JsonToken.PropertyName)
                        {
                            var prop = reader.Value as string;
                            if (prop.Equals("name", StringComparison.InvariantCultureIgnoreCase))
                            {
                                reader.Read();
                                if (reader.TokenType == JsonToken.String) name = reader.Value as string;
                            }

                            if (prop.Equals("value", StringComparison.InvariantCultureIgnoreCase))
                            {
                                reader.Read();
                                if (reader.TokenType == JsonToken.Integer) value = (int)reader.Value;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(name) || value != null)
                    {
                        object e1 = null;
                        object e2 = null;

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            Enum.TryParse(objectType, name, true, out e2);
                        }

                        if (value != null)
                            e1 = Enum.ToObject(objectType, value);

                        if (e1 != null && e2 != null && !e1.Equals(e2))
                            throw new ValidationErrorException(
                                "If the name and value properties are provided they must agree on the enum type");

                        if (e1 == null && e2 == null)
                        {
                            throw new ValidationErrorException($"An enum value must be provided for type {objectType.Name}");
                        }

                        return e1 ?? e2;
                    }
                }

                if (reader.TokenType == JsonToken.String)
                {
                    var s = reader.Value as string;
                    if (Enum.IsDefined(objectType, s))
                    {
                        if (Enum.TryParse(objectType, s, true, out var eh))
                        {
                            return eh;
                        }
                    }
                    throw new ValidationErrorException($"Cannot create enum of type{objectType.Name} from the value {s}", "");
                }
            }
            catch (ValidationErrorException)
            {
                throw;
            }
            catch (Exception)
            {
                var s = reader.Value;
                throw new ValidationErrorException($"Cannot convert {s} to type {objectType.Name}", "");
            }

            throw new ValidationErrorException("Cannot create enum value", "");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }
    }
}