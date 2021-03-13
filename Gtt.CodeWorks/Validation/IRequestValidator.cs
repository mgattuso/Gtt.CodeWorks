using System.Collections.Generic;

namespace Gtt.CodeWorks.Validation
{
    public interface IRequestValidator
    {
        IDictionary<string, string[]> Validate<T>(T request, string prefix = null);
    }
}
