using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public interface ICodeWorksTokenizer
    {
        Task Tokenize<T>(T obj, Guid correlationId) where T : class;
        bool IsEnabled { get; }
    }
}
