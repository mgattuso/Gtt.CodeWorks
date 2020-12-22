using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Web
{
    public class HttpClientConverter : IClientConverter
    {
        private readonly HttpClient _client;
        private readonly IHttpDataSerializer _dataSerializer;

        public HttpClientConverter(HttpClient client, IHttpDataSerializer dataSerializer)
        {
            _client = client;
            _dataSerializer = dataSerializer;
        }

        public async Task<ServiceResponse<TResponse>> Call<TRequest, TResponse>(TRequest request, Uri uri, HttpDataSerializerOptions options = null) where TRequest : BaseRequest where TResponse : new()
        {
            options ??= new HttpDataSerializerOptions();
            DateTimeOffset start = ServiceClock.CurrentTime();
            var payload = await _dataSerializer.SerializeRequest(request, typeof(TRequest), options);

            var requestMsg = new HttpRequestMessage(HttpMethod.Post, uri);
            requestMsg.Headers.Add("codeworks-prefs-enum", options.EnumSerializationMethod.ToString());
            requestMsg.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            var apiResponse = await _client.SendAsync(requestMsg);
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
