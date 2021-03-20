using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Tokenizer
{
    public interface IDetokenizer
    {
        Task<string> Detokenize(string token, Guid correlationId, CancellationToken cancellationToken);
        Task<string> Detokenize(TokenString token, Guid correlationId, CancellationToken cancellationToken);
        Task<DateTime?> Detokenize(TokenDate token, Guid correlationId, CancellationToken cancellationToken);
    }
}
