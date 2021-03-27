using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.StateMachines
{
    public abstract class ThirdPartyHttpCall<TRequest, TResponse> : ThirdPartyCall<TRequest, TResponse>
        where TRequest : new() where TResponse : new()
    {
        private readonly HttpClient _httpClient;

        protected ThirdPartyHttpCall(
            HttpClient httpClient, 
            IStateRepository stateRepository) : base(stateRepository)
        {
            _httpClient = httpClient;
        }

        public abstract Task<HttpRequestMessage> CreateExecutePayload(TRequest request);

        public abstract Task<HttpRequestMessage> CreateValidatePayload(TRequest request);

        public abstract Task<ExecuteAttempt> ValidateExecuteResponse(ThirdPartyHttpResponseData response);
        public abstract Task<VerificationAttempt> ValidateVerifyResponse(ThirdPartyHttpResponseData response);


        protected sealed override async Task<VerificationAttempt> Verify(TRequest request, int attempt, CancellationToken cancellationToken)
        {
            var createValidPayloadTask = CreateValidatePayload(request);
            // Catch scenario where the implement returns null directly and not a null task
            if (createValidPayloadTask == null)
            {
                return VerificationAttempt.Unsuccessful();
            }

            var payload = await createValidPayloadTask;
            if (payload == null)
            {
                return VerificationAttempt.Unsuccessful();
            }

            Data.VerifyHttpRequest = new ThirdPartyHttpRequestData
            {
                Method = payload.Method.ToString(),
                Url = payload.RequestUri.ToString(),
                Headers = payload.Headers.ToDictionary(x => x.Key, v => v.Value.ToArray()),
                Body = await payload.Content.ReadAsStringAsync()
            };

            var response = await _httpClient.SendAsync(payload, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync();

            Data.VerifyHttpResponse = new ThirdPartyHttpResponseData
            {
                StatusCode = (int)response.StatusCode,
                Headers = response.Headers.ToDictionary(x => x.Key, v => v.Value.ToArray()),
                Body = responseText
            };

            var validateVerifyResponse = ValidateVerifyResponse(Data.VerifyHttpResponse);

            if (validateVerifyResponse == null)
            {
                return VerificationAttempt.Unsuccessful();
            }

            var result = await validateVerifyResponse;
            return result;
        }

        protected sealed override async Task<ExecuteAttempt> Execute(TRequest request, int attempt, CancellationToken cancellationToken)
        {
            var payload = await CreateExecutePayload(request);
            Data.ExecuteHttpRequest = new ThirdPartyHttpRequestData
            {
                Method = payload.Method.ToString(),
                Url = payload.RequestUri.ToString(),
                Headers = payload.Headers.ToDictionary(x => x.Key, v => v.Value.ToArray()),
                Body = await payload.Content.ReadAsStringAsync()
            };

            var response = await _httpClient.SendAsync(payload, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync();

            Data.ExecuteHttpResponse = new ThirdPartyHttpResponseData
            {
                StatusCode = (int) response.StatusCode,
                Headers = response.Headers.ToDictionary(x => x.Key, v => v.Value.ToArray()),
                Body = responseText
            };

            var result = await ValidateExecuteResponse(Data.ExecuteHttpResponse);
            return result;
        }
    }
}