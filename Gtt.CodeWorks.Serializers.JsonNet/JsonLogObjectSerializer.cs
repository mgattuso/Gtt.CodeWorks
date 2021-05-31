using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Gtt.CodeWorks.Serializers.JsonNet
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
        public Task<string> Serialize<T>(T obj)
        {
            var settings = new JsonSerializerSettings()
            {
                Formatting = _debugMode ? Formatting.Indented : Formatting.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            settings.Converters.Add(new StringEnumConverter());
            settings.Converters.Add(new JsonTokenConverter());

            var result = JsonConvert.SerializeObject(obj, settings);
            return Task.FromResult(result);
        }
    }
}
