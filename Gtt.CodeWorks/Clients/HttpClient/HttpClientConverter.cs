using System;
using System.Collections.Generic;
using System.IO;
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

            if (options.EnumSerializationMethod != default)
            {
                _logger.LogTrace("Options.EnumSerializationMethod:{method} RequestId:{request}",
                    options.EnumSerializationMethod, request?.CorrelationId);
                requestMsg.Headers.Add("codeworks-prefs-enum", options.EnumSerializationMethod.ToString());
            }

            if (options.JsonSchemaValidation != default)
            {
                _logger.LogTrace("Options.JsonSchemaValidation:{method} RequestId:{request}",
                    options.JsonSchemaValidation, request?.CorrelationId);
                requestMsg.Headers.Add("codeworks-prefs-schema-check", options.JsonSchemaValidation.ToString());
            }

            if (options.IncludeDependencyMetaData != default)
            {
                _logger.LogTrace("options.IncludeDependencyMetaData:{includeDependencyMetaData} RequestId:{{request}}", 
                    options.IncludeDependencyMetaData, request?.CorrelationId);
                requestMsg.Headers.Add("codeworks-prefs-dep-meta", options.IncludeDependencyMetaData.ToString());
            }

            requestMsg.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            var apiResponse = await _client.SendAsync(requestMsg, cancellationToken);
            var responseData = await apiResponse.Content.ReadAsByteArrayAsync();

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                var contents = Encoding.UTF8.GetString(responseData);
                _logger.LogTrace("Serialized response payload {contents}, Request:{request?.CorrelationId}", contents, request?.CorrelationId);
            }

            InternalServiceResponse<TResponse> response = null;

            try
            {
                response = await _dataSerializer
                    .DeserializeResponse<InternalServiceResponse<TResponse>>(responseData);
            }
            catch (CodeWorksSerializationException ex)
            {
                return new ServiceResponse<TResponse>
                (default(TResponse), new ResponseMetaData(
                    uri.ToString(),
                    request?.CorrelationId ?? Guid.Empty,
                    ServiceResult.PermanentError,
                    (long)(ServiceClock.CurrentTime() - start).TotalMilliseconds,
                    message: ex.ToString()
                ));
            }


            Dictionary<string, ResponseMetaData> dependencies = null;

            if (options.IncludeDependencyMetaData == IncludeDependencyMetaDataStrategy.Full)
            {
                //TODO: Currently handles 1 level of dependencies - need to add recursion

                dependencies = new Dictionary<string, ResponseMetaData>
                {
                    {
                        response.MetaData.ServiceName, new ResponseMetaData(
                            response.MetaData.ServiceName,
                            response.MetaData.CorrelationId,
                            response.MetaData.Result,
                            response.MetaData.DurationMs,
                            response.MetaData.ResponseCreated,
                            response.MetaData.ErrorCodes.ToDictionary(k => Convert.ToInt32(k), v=> v.Value),
                            response.MetaData.ValidationErrors,
                            response.MetaData.Message,
                            response.MetaData.Dependencies
                        )
                    }
                };
            }

            var end = ServiceClock.CurrentTime();

            return new ServiceResponse<TResponse>(
                response.Data,
                new ResponseMetaData(
                    $"{response.MetaData.ServiceName}.Client",
                    response.MetaData.CorrelationId,
                    response.MetaData.Result,
                    (long)(end - start).TotalMilliseconds,
                    end,
                    response.MetaData.ErrorCodes.ToDictionary(k => Convert.ToInt32(k), v => v.Value),
                    response.MetaData.ValidationErrors,
                    response.MetaData.Message,
                    dependencies)); //TODO: FIX ERROR DATA

        }

        public class InternalResponseMetadata
        {
            public string ServiceName { get; set; }
            public Guid CorrelationId { get; set; }
            public ServiceResult ServiceResult { get; set; }
            public ServiceResult Result { get; set; }
            public long DurationMs { get; set; }
            public DateTimeOffset ResponseCreated { get; set; }
            public Dictionary<string, string> ErrorCodes { get; set; }
            public Dictionary<string, string[]> ValidationErrors { get; set; }
            public string Message { get; set; }
            public Dictionary<string, ResponseMetaData> Dependencies { get; set; }
        }

        public class InternalServiceResponse<TResponse> where TResponse : new()
        {
            public TResponse Data { get; set; }
            public InternalResponseMetadata MetaData { get; set; }
        }
    }
}
