using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks
{
    [Serializable]
    public class ValidationErrorException : Exception
    {
        public string Member { get; }
        public string Error { get; }

        public ValidationErrorException(string error) : this(error, "")
        {
            
        }

        public ValidationErrorException(string error, string member) : base($"Member:{member}, ValidationError:{error}")
        {
            Error = error;
            Member = member;
        }
    }
}
