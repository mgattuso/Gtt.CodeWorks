using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tokenize.Client;

namespace Gtt.CodeWorks.Tokenizer
{
    public class GttTokenizerService : ITokenizerService
    {
        private readonly ITokenizeClient _tokenizeClient;

        public GttTokenizerService(ITokenizeClient tokenizeClient)
        {
            _tokenizeClient = tokenizeClient;
        }
        public async Task<BulkToken[]> Tokenize(BulkValue[] records, Guid? correlationId)
        {
            
            if (records == null || records.Length == 0)
            {
                return new BulkToken[0];
            }

            Tokenize.Client.BulkValue[] gttValues = records.Select(x => new Tokenize.Client.BulkValue()
            {
                Correlation = x.Correlation,
                Value = x.Value
            }).ToArray();

            var gttTokens = await _tokenizeClient.Tokenize(gttValues, correlationId);

            return gttTokens.Select(x => new BulkToken
            {
                Correlation = x.Correlation,
                Token = x.Token
            }).ToArray();
        }
    }
}
