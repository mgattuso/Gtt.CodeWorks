using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public class NullTokenizer : ICodeWorksTokenizer
    {
        private NullTokenizer()
        {
            
        }

        public Task Tokenize<T>(T obj, Guid guid) where T : class
        {
            return Task.CompletedTask;
        }
        public bool IsEnabled => false;
        public static NullTokenizer SkipTokenization => new NullTokenizer();
    }
}
