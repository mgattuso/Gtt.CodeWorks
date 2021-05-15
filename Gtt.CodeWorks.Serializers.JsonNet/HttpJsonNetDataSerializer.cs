using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gtt.CodeWorks.Serializers.JsonNet
{
    public class HttpJsonNetDataSerializer : IHttpDataSerializer
    {
        public string ContentType => "application/json";
        public Encoding Encoding => Encoding.UTF8;

        public Task<string> SerializeResponse(ServiceResponse response, Type responseType, HttpDataSerializerOptions options = null)
        {

            return Task.FromResult(JsonConvert.SerializeObject(response));
        }

        public Task<T> DeserializeRequest<T>(byte[] message, HttpDataSerializerOptions options = null) where T : BaseRequest, new()
        {
            throw new NotImplementedException();
        }

        public Task<BaseRequest> DeserializeRequest(Type type, byte[] message, HttpDataSerializerOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> SerializeRequest(BaseRequest request, Type requestType, HttpDataSerializerOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<T> DeserializeResponse<T>(byte[] message, HttpDataSerializerOptions options = null) where T : new()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse> DeserializeResponse(Type type, byte[] message, HttpDataSerializerOptions options = null)
        {
            throw new NotImplementedException();
        }
    }
}
