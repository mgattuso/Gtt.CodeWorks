using System;
using System.Threading.Tasks;
using Tokenize.Client;

namespace Gtt.CodeWorks.Tokenizer
{
    public class CodeWorksTokenizer : ICodeWorksTokenizer
    {
        private readonly ITokenizeClient _tokenizeClient;

        public CodeWorksTokenizer(ITokenizeClient tokenizeClient)
        {
            _tokenizeClient = tokenizeClient ?? throw new ArgumentNullException(nameof(tokenizeClient));
        }
        public Task Tokenize<T>(T obj, Guid correlationId) where T : class
        {
            return _tokenizeClient.TokenizeObject(obj, correlationId);
        }
        public bool IsEnabled => true;
    }
}
