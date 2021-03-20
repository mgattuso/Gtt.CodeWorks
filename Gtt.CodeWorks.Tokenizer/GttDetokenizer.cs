using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tokenize.Client;

namespace Gtt.CodeWorks.Tokenizer
{
    public class GttDetokenizer : IDetokenizer
    {
        private readonly ITokenizeClient _tokenizeClient;

        public GttDetokenizer(ITokenizeClient tokenizeClient)
        {
            _tokenizeClient = tokenizeClient;
        }

        public Task<string> Detokenize(string token, Guid correlationId, CancellationToken cancellationToken)
        {
            return _tokenizeClient.Detokenize(token, correlationId);
        }

        public Task<string> Detokenize(TokenString token, Guid correlationId, CancellationToken cancellationToken)
        {
            if (token == null) return Task.FromResult((string) null);
            return _tokenizeClient.Detokenize(token.RawToken(), correlationId);
        }

        public async Task<DateTime?> Detokenize(TokenDate token, Guid correlationId, CancellationToken cancellationToken)
        {
            if (token == null) return null;
            var value = await _tokenizeClient.Detokenize(token.RawToken(), correlationId);
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (DateTime.TryParse(value, out var dt))
            {
                return dt;
            }

            return null;
        }
    }
}
