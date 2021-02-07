using System;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Tokenizer
{
    public class CodeWorksTokenizer : ICodeWorksTokenizer
    {
        private readonly ITokenizerService _tokenizerService;

        public CodeWorksTokenizer(ITokenizerService tokenizerService)
        {
            _tokenizerService = tokenizerService ?? throw new ArgumentNullException(nameof(tokenizerService));
        }

        public Task Tokenize<T>(T obj, Guid correlationId) where T : class
        {
            return ObjectTokenizer.Tokenize(obj, _tokenizerService, correlationId);
        }

        public bool IsProductionReady => true;
    }
}
