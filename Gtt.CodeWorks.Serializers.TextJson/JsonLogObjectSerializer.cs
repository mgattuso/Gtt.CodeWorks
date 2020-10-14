using System;
using System.IO;
using System.Text.Json;
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

        public async Task<string> Serialize<T>(T obj)
        {
            await using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, obj, new JsonSerializerOptions
            {
                WriteIndented = _debugMode
            });
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            var result = await reader.ReadToEndAsync();
            return result;
        }
    }
}
