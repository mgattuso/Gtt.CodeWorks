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
        Task<string> SerializeResponse(ServiceResponse response);
        Task<T> DeserializeRequest<T>(Stream message) where T : BaseRequest, new();
        Task<BaseRequest> DeserializeRequest(Type type, Stream message);
    }
}