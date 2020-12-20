using System;
using System.Collections.Generic;

namespace Gtt.CodeWorks.Validation
{
    [Serializable]
    public class ValidationErrorData
    {
        public ValidationErrorData()
        {
            Members = new List<string>();
        }

        public ValidationErrorData(string errorMessage, string[] members)
        {
            ErrorMessage = errorMessage;
            Members = members;
        }

        public string ErrorMessage { get; set; }
        public IEnumerable<string> Members { get; set; }

        public static ValidationErrorData MemberError(string member, string errorMessage)
        {
            return new ValidationErrorData(errorMessage, new[] { member });
        }

        public static ValidationErrorData MemberError(string[] members, string errorMessage)
        {
            return new ValidationErrorData(errorMessage, members);
        }

        public static ValidationErrorData GlobalError(string errorMessage)
        {
            return new ValidationErrorData(errorMessage, new string[0]);
        }
    }
}