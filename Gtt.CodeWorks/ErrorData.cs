using System;
using System.Collections.Generic;
using System.Data;
using Gtt.CodeWorks.Validation;

namespace Gtt.CodeWorks
{
    [Serializable]
    public class ErrorData
    {
        public ErrorData()
        {
            Members = new List<string>();
        }

        public ErrorData(string errorMessage) : this(errorMessage, new[] { "" })
        {

        }

        public ErrorData(string errorMessage, string member) : this(errorMessage, new[] { member })
        {

        }

        public ErrorData(string errorMessage, string[] members)
        {
            ErrorMessage = errorMessage;
            Members = members;
        }

        public ErrorData(Exception ex)
        {
            ErrorMessage = ex.ToString();
            Members = new[] { "" };
        }

        public string ErrorMessage { get; }
        public IEnumerable<string> Members { get; }

        public Dictionary<string, object> ToDictionary()
        {
            var d = new Dictionary<string, object>();
            foreach (var member in Members)
            {
                d[member] = new[] { ErrorMessage };
            }

            return d;
        }
    }
}