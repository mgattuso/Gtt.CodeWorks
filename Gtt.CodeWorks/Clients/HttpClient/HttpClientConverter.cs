using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Clients.HttpClient
{
    public class HttpClientConverter : IHttpClientConverter
    {
        private readonly System.Net.Http.HttpClient _client;
        private readonly IHttpDataSerializer _dataSerializer;
        private readonly IHttpSerializerOptionsResolver _optionsResolver;

        public HttpClientConverter(System.Net.Http.HttpClient client, IHttpDataSerializer dataSerializer, IHttpSerializerOptionsResolver optionsResolver)
        {
            _client = client;
            _dataSerializer = dataSerializer;
            _optionsResolver = optionsResolver;
        }

        public Task<ServiceResponse<TResponse>> Call<TRequest, TResponse>(TRequest request, Uri uri, CancellationToken cancellationToken) where TRequest : BaseRequest where TResponse : new()
        {
            return Call<TRequest, TResponse>(request, new Dictionary<string, object> { { "uri", uri } }, cancellationToken);
        }

        public async Task<ServiceResponse<TResponse>> Call<TRequest, TResponse>(TRequest request, IDictionary<string, object> data, CancellationToken cancellationToken) where TRequest : BaseRequest where TResponse : new()
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            Uri uri = null;
            if (data.TryGetValue("uri", out object uriObj))
            {
                uri = uriObj as Uri;
            }

            if (uri == null)
            {
                throw new Exception("Expected a data entry called \"uri\" of the type Uri with a reference to the Http Service URL");
            }

            var options = _optionsResolver?.Options() ?? new HttpDataSerializerOptions();
            DateTimeOffset start = ServiceClock.CurrentTime();
            var payload = await _dataSerializer.SerializeRequest(request, typeof(TRequest), options);

            var requestMsg = new HttpRequestMessage(HttpMethod.Post, uri);
            requestMsg.Headers.Add("codeworks-prefs-enum", options.EnumSerializationMethod.ToString());
            requestMsg.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            var apiResponse = await _client.SendAsync(requestMsg, cancellationToken);
            var responseStream = await apiResponse.Content.ReadAsStreamAsync();
            var response = await _dataSerializer.DeserializeResponse<InternalServiceResponse<TResponse>>(responseStream);
            return new ServiceResponse<TResponse>(response.Data, new ResponseMetaData(response.MetaData.ServiceName, start, response.MetaData.CorrelationId, response.MetaData.Result, response.MetaData.Errors)); //TODO: FIX ERROR DATA

        }

        public class InternalResponseMetadata
        {
            public string ServiceName { get; set; }
            public Guid CorrelationId { get; set; }
            public ServiceResult Result { get; set; }
            public long DurationMs { get; set; }
            public DateTimeOffset ResponseCreated { get; set; }
            public Dictionary<string, string[]> Errors { get; set; }
            public Dictionary<string, ResponseMetaData> Dependencies { get; set; }
        }

        public class InternalServiceResponse<TResponse> where TResponse : new()
        {
            public TResponse Data { get; set; }
            public InternalResponseMetadata MetaData { get; set; }
        }
    }
}
