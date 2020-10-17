using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Serializers.TextJson
{
    public class HttpJsonDataSerializer : IHttpDataSerializer
    {
        private readonly bool _debugMode;

        public HttpJsonDataSerializer()
        {
            _debugMode = true;
        }

        public string ContentType => "application/json";
        public Encoding Encoding => Encoding.UTF8;
        public async Task<string> SerializeResponse<T>(ServiceResponse<T> response) where T : new()
        {
            var opts = new JsonSerializerOptions
            {
                WriteIndented = _debugMode,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true
            };
            opts.Converters.Add(new JsonStringEnumConverter());

            await using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, response, opts);

            stream.Position = 0;
            using var reader = new StreamReader(stream);
            var result = await reader.ReadToEndAsync();
            return result;
        }

        public async Task<T> DeserializeRequest<T>(Stream stream) where T : BaseRequest, new()
        {
            var opts = new JsonSerializerOptions
            {
                IgnoreNullValues = false,
                PropertyNameCaseInsensitive = true,
            };

            try
            {
                var result = await JsonSerializer.DeserializeAsync<T>(stream, opts);
                return result;
            }
            catch (JsonException)
            {
                return new T();
            }
        }
    }
}
