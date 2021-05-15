using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Tokenizer;

namespace Gtt.CodeWorks.SampleServices
{
    public class TokenizedDataService : BaseServiceInstance<TokenizedDataRequest, TokenizedDataResponse>
    {
        private readonly IDetokenizer _detokenizer;

        public TokenizedDataService(
            IDetokenizer detokenizer,
            CoreDependencies coreDependencies) : base(coreDependencies)
        {
            _detokenizer = detokenizer;
        }

        protected override async Task<ServiceResponse<TokenizedDataResponse>> Implementation(TokenizedDataRequest request, CancellationToken cancellationToken)
        {
            var raw = await _detokenizer.Detokenize(request.Ssn, request.CorrelationId, cancellationToken);
            var rawDate = await _detokenizer.Detokenize(request.Date, request.CorrelationId, cancellationToken);

            var response = new TokenizedDataResponse
            {
                Ssn = request.Ssn,
                Raw = raw,
                Date = rawDate
            };

            return Successful(response);
        }

        protected override Task<string> CreateDistributedLockKey(TokenizedDataRequest request, CancellationToken cancellationToken)
        {
            return NoDistributedLock();
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }

    }

    public class TokenizedDataRequest : BaseRequest
    {
        [Required]
        public TokenString Ssn { get; set; }

        public TokenDate Date { get; set; }
    }

    public class TokenizedDataResponse
    {
        [AlwaysPresent]
        public TokenString Ssn { get; set; }

        public string Raw { get; set; }
        public DateTime? Date { get; set; }
    }
}
