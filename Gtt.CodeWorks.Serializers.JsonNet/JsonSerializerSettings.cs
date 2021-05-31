using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Gtt.CodeWorks.Serializers.JsonNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Gtt.CodeWorks.Serializers.JsonNet
{
    public static class JsonNetSerializerSettings
    {
        public static JsonSerializerSettings CreateJsonSerializerSettings(HttpDataSerializerOptions options, bool debugMode)
        {
            options = options ?? new HttpDataSerializerOptions();

            var settings = new JsonSerializerSettings();
            settings.Formatting = debugMode ? Formatting.Indented : Formatting.None;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.NullValueHandling = NullValueHandling.Ignore;

            settings.Converters.Add(new JsonTokenConverter());

            switch (options.EnumSerializationMethod)
            {
                case EnumSerializationMethod.String:
                    settings.Converters.Add(new StringEnumConverter());
                    break;
                case EnumSerializationMethod.Numeric:
                    // NOTHING NEEDED NUMERIC CONVERSION BY DEFAULT
                    break;
                case EnumSerializationMethod.Object:
                    settings.Converters.Add(new JsonEnumObjectConverter());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return settings;
        }
    }

    //public static class TextJsonSerializerSettings
    //{
    //    public static JsonSerializerOptions CreateJsonSerializerOptions(HttpDataSerializerOptions options, bool debugMode)
    //    {
    //        options = options ?? new HttpDataSerializerOptions();
    //        var opts = new JsonSerializerOptions
    //        {
    //            WriteIndented = debugMode,
    //            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
    //            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //            IgnoreNullValues = true,
    //            PropertyNameCaseInsensitive = true
    //        };

    //        opts.Converters.Add(new JsonTokenConverterFactory());

    //        if (options.EnumSerializationMethod == EnumSerializationMethod.Object)
    //            opts.Converters.Add(new JsonEnumConverterFactory());
    //        if (options.EnumSerializationMethod == EnumSerializationMethod.String)
    //            opts.Converters.Add(new JsonStringEnumConverter());

    //        return opts;
    //    }
    //}
}
