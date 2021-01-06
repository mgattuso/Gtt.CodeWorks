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
        Task<T> DeserializeRequest<T>(Stream message, HttpDataSerializerOptions options = null) where T : BaseRequest, new();
        Task<BaseRequest> DeserializeRequest(Type type, Stream message, HttpDataSerializerOptions options = null);
        Task<string> SerializeRequest(BaseRequest request, Type requestType, HttpDataSerializerOptions options = null);
        Task<T> DeserializeResponse<T>(Stream message, HttpDataSerializerOptions options = null) where T : new();
        Task<ServiceResponse> DeserializeResponse(Type type, Stream message, HttpDataSerializerOptions options = null);
        Task<IDictionary<string, string[]>> ValidateSchema(Stream contents, Type type);
    }
}