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

    public class NullTokenService : ITokenizerService
    {
        public Task<BulkToken[]> Tokenize(BulkValue[] records, Guid? correlationId)
        {
            var results = new BulkToken[records.Length];
            for (var i = 0; i < records.Length; i++)
            {
                var record = records[i];
                results[i] = new BulkToken
                {
                    Token = record.Value,
                    Correlation = record.Correlation
                };
            }

            return Task.FromResult(results);
        }
    }
}
