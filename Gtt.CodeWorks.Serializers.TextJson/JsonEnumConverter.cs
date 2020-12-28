using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gtt.CodeWorks.Serializers.TextJson
{
    public class JsonEnumConverter : JsonConverterFactory
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

    public class JsonEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsEnum;
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                if (reader.TokenType == JsonTokenType.Number)
                {
                    var intV = reader.GetInt32();
                    if (Enum.IsDefined(typeToConvert, intV))
                    {
                        return (T)Enum.ToObject(typeToConvert, intV);
                    }
                    throw new ValidationErrorException($"Cannot create enum of type{typeToConvert.Name} from the value {intV}", "");
                }

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var nextToken = reader.TokenType;
                    var name = "";
                    int? value = null;
                    while (nextToken != JsonTokenType.EndObject)
                    {
                        reader.Read();
                        nextToken = reader.TokenType;
                        if (nextToken == JsonTokenType.PropertyName)
                        {
                            var prop = reader.GetString();
                            if (prop.Equals("name", StringComparison.InvariantCultureIgnoreCase))
                            {
                                reader.Read();
                                if (reader.TokenType == JsonTokenType.String) name = reader.GetString();
                            }

                            if (prop.Equals("value", StringComparison.InvariantCultureIgnoreCase))
                            {
                                reader.Read();
                                if (reader.TokenType == JsonTokenType.Number) value = reader.GetInt32();
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(name) || value != null)
                    {
                        T? e1 = null;
                        T? e2 = null;

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            if (Enum.TryParse(name, true, out T eh))
                            {
                                e2 = eh;
                            }
                        }

                        if (value != null) 
                            e1 = (T)Enum.ToObject(typeof(T), value);

                        if (e1 != null && e2 != null && !e1.Value.Equals(e2.Value))
                            throw new ValidationErrorException(
                                "If the name and value properties are provided they must agree on the enum type");

                        if (e1 == null && e2 == null)
                        {
                            throw new ValidationErrorException($"An enum value must be provided for type {typeof(T).Name}");
                        }

                        return e1 ?? e2.Value;
                    }
                }

                if (reader.TokenType == JsonTokenType.String)
                {
                    var s = reader.GetString();
                    if (Enum.IsDefined(typeToConvert, s))
                    {
                        if (Enum.TryParse(s, true, out T eh))
                        {
                            return eh;
                        }
                    }
                    throw new ValidationErrorException($"Cannot create enum of type{typeToConvert.Name} from the value {s}", "");
                }
            }
            catch (ValidationErrorException)
            {
                throw;
            }
            catch (Exception)
            {
                var s = reader.GetString();
                throw new ValidationErrorException($"Cannot convert {s} to type {typeToConvert.Name}", "");
            }

            throw new ValidationErrorException("Cannot create enum value", "");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var v = Convert.ToInt32(value);
            var description = value.ToDescription();

            writer.WriteStartObject();
            writer.WritePropertyName(options.PropertyNamingPolicy.ConvertName("Name"));
            writer.WriteStringValue(value.ToString());
            writer.WritePropertyName(options.PropertyNamingPolicy.ConvertName("Value"));
            writer.WriteNumberValue(v);
            writer.WritePropertyName(options.PropertyNamingPolicy.ConvertName("Description"));
            writer.WriteStringValue(description);
            writer.WriteEndObject();
        }
    }
}