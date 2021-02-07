using System;
using System.Linq;
using System.Text;
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

            var tokens = await tokenizerService.Tokenize(records, correlationId);

            var tokenDict = tokens.ToDictionary(x => Convert.ToInt32(x.Correlation));

            foreach (var t in tokenizableFields)
            {
                var v = t.Value;
                var tok = tokenDict[v.Correlation];
                var sa = v.SensitiveAttribute;
                string mask = v.Value.MaskedValue;

                if (sa != null)
                {
                    mask = t.Value.Value.ValueToStoredFormat().ToMasked(sa.Reveal, sa.FirstChars, sa.LastChars);
                }

                v.Value.SetTokenAndMask(tok.Token, mask);
            }
        }

        public static async Task Detokenize(object obj, IDetokenizer tokenizer, Guid? correlationId = null)
        {
            var tokenizableFields = await TokenizeModel.CollectTokenizablesAll(obj);
            if (tokenizableFields.Count == 0)
            {
                return;
            }

            var values = await tokenizer.Detokenize(tokenizableFields
                .Where(x => x.Value.Value.HasToken())
                .Select(x => new BulkToken
                {
                    Token = x.Value.Value.RawToken(),
                    Correlation = x.Key.ToString()
                })
                .ToArray(), correlationId);

            if (values.Length == 0)
            {
                return;
            }

            var valuesDict = values.ToDictionary(x => Convert.ToInt32(x.Correlation));

            foreach (var t in tokenizableFields)
            {
                var v = t.Value;
                var val = valuesDict.GetValueOrDefault(v.Correlation);
                if (val != null)
                {
                    v.Value.ValueFromStoredFormat(val.Value);
                }
            }
        }
    }
}
