using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public interface IHttpDataSerializer
    {
        string ContentType { get; }
        Encoding Encoding { get; }
        Task<string> SerializeResponse(ServiceResponse response, Type responseType, HttpDataSerializerOptions options = null);
        Task<T> DeserializeRequest<T>(byte[] message, HttpDataSerializerOptions options = null) where T : BaseRequest, new();
        Task<BaseRequest> DeserializeRequest(Type type, byte[] message, HttpDataSerializerOptions options = null);
        Task<string> SerializeRequest(BaseRequest request, Type requestType, HttpDataSerializerOptions options = null);
        Task<T> DeserializeResponse<T>(byte[] message, HttpDataSerializerOptions options = null) where T : new();
        Task<ServiceResponse> DeserializeResponse(Type type, byte[] message, HttpDataSerializerOptions options = null);
        Task<IDictionary<string, object>> ValidateSchema(byte[] contents, Type type, HttpDataSerializerOptions options = null);
    }
}