using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks.Validation
{
    public static class ValidationHelper
    {
        public static IDictionary<string, string[]> Create(string error, string member)
        {
            return Create(error, new[] {member});
        }

        public static IDictionary<string, string[]> Create(string error, string[] members)
        {
            return new Dictionary<string, string[]>
            {
                {error, members}
            };
        }
    }
}
