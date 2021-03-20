using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Tokenizer
{
    public interface ITokenizerService
    {
        Task<BulkToken[]> Tokenize(BulkValue[] records, Guid? correlationId);
    }
}
