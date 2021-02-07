using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gtt.CodeWorks.Serializers.TextJson
{
    public static class TextJsonSerializerSettings
    {
        public static JsonSerializerOptions CreateJsonSerializerOptions(HttpDataSerializerOptions options, bool debugMode)
        {
            options = options ?? new HttpDataSerializerOptions();
            var opts = new JsonSerializerOptions
            {
                WriteIndented = debugMode,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true
            };

            opts.Converters.Add(new JsonTokenConverterFactory());

            if (options.EnumSerializationMethod == EnumSerializationMethod.Object)
                opts.Converters.Add(new JsonEnumConverterFactory());
            if (options.EnumSerializationMethod == EnumSerializationMethod.String)
                opts.Converters.Add(new JsonStringEnumConverter());

            return opts;
        }
    }
}
