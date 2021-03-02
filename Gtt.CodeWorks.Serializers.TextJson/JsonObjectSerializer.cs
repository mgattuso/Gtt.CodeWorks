using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Serializers.TextJson
{
    public class JsonObjectSerializer : IObjectSerializer
    {
        private readonly bool _debugMode;

        public JsonObjectSerializer(bool debugMode)
        {
            _debugMode = debugMode;
        }

        public async Task<string> Serialize(Type t, object obj)
        {
            var opts = GetOptions();

            using (var stream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(stream, obj, t, opts, CancellationToken.None);
                stream.Position = 0;

                using (var reader = new StreamReader(stream))
                {
                    var result = await reader.ReadToEndAsync();
                    return result;
                }
            }
        }

        public Task<string> Serialize<T>(T obj)
        {
            return Serialize(typeof(T), obj);
        }

        public async Task<T> Deserialize<T>(string str)
        {
            var a= await Deserialize(typeof(T), str);
            return (T)a;
        }

        public async Task<object> Deserialize(Type t, string str)
        {
            var opts = GetOptions();
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var ms = new MemoryStream(bytes))
            {
                var a = await JsonSerializer.DeserializeAsync(ms, t, opts, CancellationToken.None);
                return a;
            }
        }

        private JsonSerializerOptions GetOptions()
        {
            var opts = new JsonSerializerOptions
            {
                WriteIndented = _debugMode,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            opts.Converters.Add(new JsonStringEnumConverter());
            opts.Converters.Add(new JsonTokenConverterFactory());
            return opts;
        }
    }
}
