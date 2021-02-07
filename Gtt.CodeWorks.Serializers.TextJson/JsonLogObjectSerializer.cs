using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Serializers.TextJson
{
    public class JsonLogObjectSerializer : ILogObjectSerializer
    {
        private readonly bool _debugMode;

        public JsonLogObjectSerializer(bool debugMode)
        {
            _debugMode = debugMode;
        }

        public JsonLogObjectSerializer()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns>
        ///  JSON serialized object.
        ///  A null input type return the string "null".
        ///  A string literal returns the quoted string "message"
        /// </returns>
        public async Task<string> Serialize<T>(T obj)
        {
            var opts = new JsonSerializerOptions
            {
                WriteIndented = _debugMode,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            opts.Converters.Add(new JsonStringEnumConverter());
            opts.Converters.Add(new JsonTokenConverterFactory());

            using (var stream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(stream, obj, opts);
                stream.Position = 0;

                using (var reader = new StreamReader(stream))
                {
                    var result = await reader.ReadToEndAsync();
                    return result;
                }
            }
        }
    }
}
