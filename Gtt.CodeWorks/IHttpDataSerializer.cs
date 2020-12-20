using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public interface IHttpDataSerializer
    {
        string ContentType { get; }
        Encoding Encoding { get; }
        Task<string> SerializeResponse(ServiceResponse response, Type responseType);
        Task<T> DeserializeRequest<T>(Stream message) where T : BaseRequest, new();
        Task<BaseRequest> DeserializeRequest(Type type, Stream message);
        Task<string> SerializeRequest(BaseRequest request, Type requestType);
        Task<T> DeserializeResponse<T>(Stream message) where T : new();
        Task<ServiceResponse> DeserializeResponse(Type type, Stream message);
    }
}