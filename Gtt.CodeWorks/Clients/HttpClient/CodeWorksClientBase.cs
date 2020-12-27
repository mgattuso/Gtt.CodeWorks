using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Clients.HttpClient
{
    public abstract class CodeWorksClientBase
    {
        private readonly CodeWorksClientEndpoint _endpoint;
        private readonly System.Net.Http.HttpClient _client;
        private readonly IHttpDataSerializer _httpDataSerializer;
        private readonly IHttpSerializerOptionsResolver _optionsResolver;

        protected CodeWorksClientBase(
            CodeWorksClientEndpoint endpoint, 
            System.Net.Http.HttpClient client, 
            IHttpDataSerializer httpDataSerializer,
            IHttpSerializerOptionsResolver optionsResolver)
        {
            _endpoint = endpoint;
            _client = client;
            _httpDataSerializer = httpDataSerializer;
            _optionsResolver = optionsResolver;
        }

        protected Task<ServiceResponse<TResponse>> Call<TRequest, TResponse>(string serviceRoute, TRequest request, CancellationToken cancellationToken) 
            where TRequest : BaseRequest
            where TResponse : new()
        {
            var converter = new HttpClientConverter(_client, _httpDataSerializer, _optionsResolver);
            var url = _endpoint.GetUrl(serviceRoute);
            return converter.Call<TRequest, TResponse>(request, url, cancellationToken);
        }
    }
}