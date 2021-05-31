using System;

namespace Gtt.CodeWorks.Serializers.JsonNet
{
    //public class JsonTokenConverter : JsonConverter<ITokenizable>
    //{
    //    public override ITokenizable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        string val = reader.GetString();

    //        if (typeToConvert == typeof(TokenString))
    //        {
    //            return new TokenString(val);
    //        }

    //        if (typeToConvert == typeof(TokenDate))
    //        {
    //            return new TokenDate(val);
    //        }

    //        throw new NotImplementedException($"No converter exists for {typeToConvert.FullName}");
    //    }

    //    public override void Write(Utf8JsonWriter writer, ITokenizable value, JsonSerializerOptions options)
    //    {
    //        var str = value.ToString();
    //        writer.WriteStringValue(str);
    //    }
    //}
}