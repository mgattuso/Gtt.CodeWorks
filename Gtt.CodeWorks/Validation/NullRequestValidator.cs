using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks.Validation
{
    public class NullRequestValidator : IRequestValidator
    {
        private NullRequestValidator()
        {
            
        }

        public IDictionary<string, string[]> Validate<T>(T request, string prefix = null)
        {
            return new Dictionary<string, string[]>();
        }

        public static NullRequestValidator Instance => new NullRequestValidator();
    }
}
