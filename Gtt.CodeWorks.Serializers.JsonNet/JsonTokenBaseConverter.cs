using System;

namespace Gtt.CodeWorks.Serializers.JsonNet
{
    //public class JsonTokenBaseConverter : JsonConverter<TokenBase>
    //{
    //    public override TokenBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        string val = reader.GetString();

    //        if (typeToConvert == typeof(ClientTokenString))
    //        {
    //            ClientTokenString cts = val;
    //            return cts;
    //        }

    //        if (typeToConvert == typeof(ClientTokenDate))
    //        {
    //            ClientTokenDate ctd = val;
    //            return ctd;
    //        }

    //        throw new NotImplementedException($"No converter exists for {typeToConvert.FullName}");
    //    }

    //    public override void Write(Utf8JsonWriter writer, TokenBase value, JsonSerializerOptions options)
    //    {
    //        var str = value.ToString();
    //        writer.WriteStringValue(str);
    //    }
    //}
}