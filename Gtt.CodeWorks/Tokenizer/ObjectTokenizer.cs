using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Tokenizer
{
    public static class ObjectTokenizer
    {
        public static async Task Tokenize(object obj, ITokenizerService tokenizerService, Guid? correlationId = null)
        {
            var tokenizableFields = await TokenizeModel.CollectTokenizablesAll(obj);

            if (tokenizableFields.Count == 0)
            {
                return;
            }

            BulkValue[] records = tokenizableFields
                .Where(x => !x.Value.Value.HasToken())
                .Select(x => new BulkValue
                {
                    Value = x.Value.Value.ValueToStoredFormat(),
                    Correlation = x.Key.ToString()
                })
                .ToArray();

            // SEND ALL NON-TOKENIZED FIELDS TO THE SERVICE
            var tokens = await tokenizerService.Tokenize(records, correlationId);

            var tokenDict = tokens.ToDictionary(x => Convert.ToInt32(x.Correlation));

            foreach (var t in tokenizableFields)
            {
                var v = t.Value;
                
                // RETRIEVE THE TOKENIZED DATA FROM RESULTS FROM THE SERVICE - WILL BE NULL IF THE DATA
                // WAS ALREADY TOKENIZED
                BulkToken tok = tokenDict.GetValueOrDefault(v.Correlation);

                // IF THE DATA WAS ALREADY TOKENIZED THEN POPULATE IT FROM THE DATA YOU ALREADY HAVE
                if (tok == null)
                {
                    tok = new BulkToken
                    {
                        Correlation = v.Correlation.ToString(),
                        Token = v.Value.RawToken()
                    };

                    // LAST CHECK - IF THE TOKEN DOES NOT EXIST AT THIS POINT THEN THROW AN ERROR
                    if (string.IsNullOrWhiteSpace(tok.Token))
                    {
                        throw new Exception("Could not find an appropriate token");
                    }
                }

                var sa = v.SensitiveAttribute;
                string mask = v.Value.MaskedValue;

                if (sa != null)
                {
                    mask = t.Value.Value.ValueToStoredFormat().ToMasked(sa.Reveal, sa.FirstChars, sa.LastChars);
                }

                v.Value.SetTokenAndMask(tok.Token, mask);
            }
        }
    }
}
