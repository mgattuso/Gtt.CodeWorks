using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.StateMachines;

namespace Gtt.CodeWorks.SampleServices
{
    public class ThirdPartyService : BaseServiceInstance<ThirdPartyRequest, ThirdPartyResponse>
    {
        private readonly IStateRepository _stateRepository;

        public ThirdPartyService(CoreDependencies coreDependencies, IStateRepository stateRepository) : base(coreDependencies)
        {
            _stateRepository = stateRepository;
        }

        protected override async Task<ServiceResponse<ThirdPartyResponse>> Implementation(ThirdPartyRequest request, CancellationToken cancellationToken)
        {
            var exampleCall = new ExampleService(_stateRepository);
            var result = await exampleCall.Process(new ThirdPartyCallRequest
            {
                Name = "Test Request"
            }, $"example-{request.CorrelationId}", CorrelationId);
            if (result != null)
            {
                var r = new ThirdPartyResponse();
                return Successful(r);
            }

            return TemporaryException("Error calling third party service");
        }

        protected override Task<string> CreateDistributedLockKey(ThirdPartyRequest request, CancellationToken cancellationToken)
        {
            return NoDistributedLock();
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }

    }

    public class ThirdPartyRequest : BaseRequest
    {

    }

    public class ThirdPartyResponse
    {

    }

    public class ExampleService : ThirdPartyCall<ThirdPartyCallRequest, ThirdPartCallResponse>
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        public ExampleService(IStateRepository stateRepository) : base(stateRepository)
        {
        }

        protected override async Task<ExecuteAttempt> Execute(ThirdPartyCallRequest request, int attempt, CancellationToken cancellationToken)
        {
            if (attempt <= 1)
            {
                await Task.Delay(4000, cancellationToken);
            }

            var result = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://example.com"), cancellationToken);

            if (result.IsSuccessStatusCode)
            {
                var response = new ThirdPartCallResponse
                {
                    StatusCode = (int)result.StatusCode
                };
                return ExecuteAttempt.Successful(response);
            }

            return ExecuteAttempt.Unsuccessful();
        }

        protected override Task<VerificationAttempt> Verify(ThirdPartyCallRequest request, int attempt, CancellationToken cancellationToken)
        {
            var response = new ThirdPartCallResponse
            {
                StatusCode = 200
            };
            return Task.FromResult(VerificationAttempt.Successful(response));
        }

        protected override int MaxAttempts()
        {
            return 3;
        }

        protected override int ExecuteCallTimeoutMs()
        {
            return 3000;
        }

        protected override bool AllowManualOverride()
        {
            return true;
        }

        protected override int DelayBetweenAttemptsMs()
        {
            return 1000;
        }
    }

    public class ThirdPartyCallRequest
    {
        public string Name { get; set; }
    }

    public class ThirdPartCallResponse
    {
        public int StatusCode { get; set; }
    }
}
