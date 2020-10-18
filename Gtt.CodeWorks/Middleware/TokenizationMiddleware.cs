using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Middleware
{
    public class TokenizationMiddleware : IServiceMiddleware
    {
        private readonly ICodeWorksTokenizer _tokenizer;
        private readonly CodeWorksEnvironment _environment;

        public TokenizationMiddleware(ICodeWorksTokenizer tokenizer, IServiceEnvironmentResolver environmentResolver)
        {
            _tokenizer = tokenizer;
            _environment = environmentResolver.Environment;
        }

        public async Task<ServiceResponse> OnRequest<TReq>(IServiceInstance service, TReq request, CancellationToken cancellationToken) where TReq : BaseRequest, new()
        {
            if (_environment == CodeWorksEnvironment.Production && !_tokenizer.IsProductionReady)
            {
                throw new Exception($"Tokenizer {_tokenizer.GetType()} must be enabled in production");
            }
            await _tokenizer.Tokenize(request, request.CorrelationId);
            return this.ContinuePipeline();
        }

        public Task OnResponse<TReq, TRes>(IServiceInstance service, TReq request, ServiceResponse<TRes> response) where TReq : BaseRequest, new() where TRes : new()
        {
            return Task.CompletedTask;
        }

        public bool IgnoreExceptions => false;
    }
}
