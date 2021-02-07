using System;
using System.Threading.Tasks;
using Gtt.CodeWorks.Tokenizer;

namespace Gtt.CodeWorks
{
    public interface IDetokenizer
    {
        Task<string> Detokenize(string token, Guid? correlationId = null);
        Task<BulkValue[]> Detokenize(BulkToken[] tokens, Guid? correlationId = null);
    }
}