using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Gtt.CodeWorks.Clients.HttpClient
{
    public class HttpClientConverter : IHttpClientConverter
    {
        private readonly System.Net.Http.HttpClient _client;
        private readonly IHttpDataSerializer _dataSerializer;
        private readonly IHttpSerializerOptionsResolver _optionsResolver;
        private readonly ILogger _logger;

        public HttpClientConverter(
            System.Net.Http.HttpClient client,
            IHttpDataSerializer dataSerializer,
            IHttpSerializerOptionsResolver optionsResolver,
            ILogger<HttpClientConverter> logger)
        {
            _client = client;
            _dataSerializer = dataSerializer;
            _optionsResolver = optionsResolver;
            _logger = logger;
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

            _logger.LogTrace("Calling Uri:{uri}, Request:{request?.CorrelationId}", uri, request?.CorrelationId);

            var options = _optionsResolver?.Options() ?? new HttpDataSerializerOptions();
            DateTimeOffset start = ServiceClock.CurrentTime();

            _logger.LogTrace("Serializing request payload {request}, Request:{request?.CorrelationId}", request, request?.CorrelationId);
            var payload = await _dataSerializer.SerializeRequest(request, typeof(TRequest), options);

            _logger.LogTrace("Serialized request payload {payload}, Request:{request?.CorrelationId}", payload, request?.CorrelationId);

            var requestMsg = new HttpRequestMessage(HttpMethod.Post, uri);
            _logger.LogTrace("Options.EnumSerializationMethod {method}", options.EnumSerializationMethod);
            requestMsg.Headers.Add("codeworks-prefs-enum", options.EnumSerializationMethod.ToString());

            _logger.LogTrace("options.IncludeDependencyMetaData:{includeDependencyMetaData} RequestId:{{request}}", options.IncludeDependencyMetaData, request?.CorrelationId);

            if (options.IncludeDependencyMetaData)
            {
                requestMsg.Headers.Add("codeworks-prefs-dep-meta", "full");
            }

            requestMsg.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            var apiResponse = await _client.SendAsync(requestMsg, cancellationToken);
            var responseStream = await apiResponse.Content.ReadAsStreamAsync();
            InternalServiceResponse<TResponse> response = null;

            try
            {
                response = await _dataSerializer
                    .DeserializeResponse<InternalServiceResponse<TResponse>>(responseStream);
            }
            catch (CodeWorksSerializationException ex)
            {
                return new ServiceResponse<TResponse>
                (default(TResponse), new ResponseMetaData(
                    uri.ToString(),
                    ServiceClock.CurrentTime(),
                    (long)(ServiceClock.CurrentTime() - start).TotalMilliseconds,
                    request?.CorrelationId ?? Guid.Empty,
                    ServiceResult.PermanentError,
                    new Dictionary<string, string[]>
                    {
                        {"Error", new [] { ex.ToString() }}
                    },
                    null
                ));
            }


            Dictionary<string, ResponseMetaData> dependencies = null;

            if (options.IncludeDependencyMetaData)
            {
                //TODO: Currently handles 1 level of dependencies - need to add recursion

                dependencies = new Dictionary<string, ResponseMetaData>
                {
                    {
                        response.MetaData.ServiceName, new ResponseMetaData(
                            response.MetaData.ServiceName,
                            response.MetaData.ResponseCreated,
                            response.MetaData.DurationMs,
                            response.MetaData.CorrelationId,
                            response.MetaData.Result,
                            response.MetaData.Errors
                        )
                    }
                };
            }

            var end = ServiceClock.CurrentTime();

            return new ServiceResponse<TResponse>(
                response.Data,
                new ResponseMetaData(
                    $"{response.MetaData.ServiceName}.Client",
                    end,
                    (long)(end - start).TotalMilliseconds,
                    response.MetaData.CorrelationId,
                    response.MetaData.Result,
                    response.MetaData.Errors,
                    dependencies)); //TODO: FIX ERROR DATA

        }

        public class InternalResponseMetadata
        {
            public string ServiceName { get; set; }
            public Guid CorrelationId { get; set; }
            public ServiceResult Result { get; set; }
            public long DurationMs { get; set; }
            public DateTimeOffset ResponseCreated { get; set; }
            public Dictionary<string, string[]> Errors { get; set; }
            public Dictionary<string, InternalResponseMetadata> Dependencies { get; set; }
        }

        public class InternalServiceResponse<TResponse> where TResponse : new()
        {
            public TResponse Data { get; set; }
            public InternalResponseMetadata MetaData { get; set; }
        }
    }
}
