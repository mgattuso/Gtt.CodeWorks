using System.Net.Http;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Web
{
    public abstract class CodeWorksClientBase
    {
        private readonly CodeWorksClientEndpoint _endpoint;
        private readonly HttpClient _client;
        private readonly IHttpDataSerializer _httpDataSerializer;

        protected CodeWorksClientBase(CodeWorksClientEndpoint endpoint, HttpClient client, IHttpDataSerializer httpDataSerializer)
        {
            _endpoint = endpoint;
            _client = client;
            _httpDataSerializer = httpDataSerializer;
        }

        protected Task<ServiceResponse<TResponse>> Call<TRequest, TResponse>(string serviceRoute, TRequest request) 
            where TRequest : BaseRequest
            where TResponse : new()
        {
            var converter = new HttpClientConverter(_client, _httpDataSerializer);
            var url = _endpoint.GetUrl(serviceRoute);
            return converter.Call<TRequest, TResponse>(request, url);
        }
    }
}