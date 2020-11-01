using System;
using System.Collections.Generic;
using System.Data;

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

        public string ErrorMessage { get; set; }
        public IEnumerable<string> Members { get; set; }

        public Dictionary<string, string[]> ToDictionary()
        {
            var d = new Dictionary<string, string[]>();
            foreach (var member in Members)
            {
                d[member] = new[] { ErrorMessage };
            }

            return d;
        }
    }
}