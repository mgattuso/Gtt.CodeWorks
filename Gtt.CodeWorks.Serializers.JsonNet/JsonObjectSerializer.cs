using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Gtt.CodeWorks.Serializers.JsonNet
{
    public class JsonObjectSerializer : IObjectSerializer
    {
        private readonly bool _debugMode;

        public JsonObjectSerializer(bool debugMode)
        {
            _debugMode = debugMode;
        }

        public Task<string> Serialize(Type t, object obj)
        {
            var settings = GetSettings();
            var result = JsonConvert.SerializeObject(obj, settings);
            return Task.FromResult(result);
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

        public Task<object> Deserialize(Type t, string str)
        {
            var settings = GetSettings();
            var obj = JsonConvert.DeserializeObject(str, t, settings);
            return Task.FromResult(obj);
        }

        private JsonSerializerSettings GetSettings()
        {
            var settings =  new JsonSerializerSettings
            {
                Formatting = _debugMode ? Formatting.Indented : Formatting.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            settings.Converters.Add(new StringEnumConverter());
            settings.Converters.Add(new JsonTokenConverter());

            return settings;
        }
    }
}
