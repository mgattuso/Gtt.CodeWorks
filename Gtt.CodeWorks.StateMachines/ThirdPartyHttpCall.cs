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

        public abstract HttpRequestMessage CreateExecutePayload(TRequest request);

        public abstract HttpRequestMessage CreateValidatePayload(TRequest request);

        public abstract ExecuteAttempt ValidateExecuteResponse(ThirdPartyHttpResponseData response);
        public abstract VerificationAttempt ValidateVerifyResponse(ThirdPartyHttpResponseData response);


        protected sealed override async Task<VerificationAttempt> Verify(TRequest request, int attempt, CancellationToken cancellationToken)
        {
            var payload = CreateValidatePayload(request);
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

            return ValidateVerifyResponse(Data.VerifyHttpResponse);
        }

        protected sealed override async Task<ExecuteAttempt> Execute(TRequest request, int attempt, CancellationToken cancellationToken)
        {
            var payload = CreateExecutePayload(request);
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

            return ValidateExecuteResponse(Data.ExecuteHttpResponse);
        }
    }
}