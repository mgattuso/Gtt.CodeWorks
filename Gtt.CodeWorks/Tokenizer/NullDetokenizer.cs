using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Tokenizer
{
    public class NullDetokenizer : IDetokenizer
    {
        public Task<string> Detokenize(TokenString token, Guid correlationId, CancellationToken cancellationToken)
        {
            return Task.FromResult(token.RawToken());
        }

        public Task<DateTime?> Detokenize(TokenDate token, Guid correlationId, CancellationToken cancellationToken)
        {
            var tok = token.RawToken();
            if (string.IsNullOrWhiteSpace(tok)) return null;
            DateTime? dt = DateTime.Parse(tok);
            return Task.FromResult(dt);
        }

        public Task<string> Detokenize(string token, Guid correlationId, CancellationToken cancellationToken)
        {
            TokenString ts = token;
            return Detokenize(ts, correlationId, cancellationToken);
        }
    }
}