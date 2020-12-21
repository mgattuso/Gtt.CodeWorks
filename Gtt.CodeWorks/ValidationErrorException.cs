using System;
using System.Text;

namespace Gtt.CodeWorks
{
    [Serializable]
    public class ValidationErrorException : Exception
    {
        public ErrorData Error { get; }

        public ValidationErrorException(string error, string member) 
            : this(new ErrorData(error, member))
        {

        }

        public ValidationErrorException(string error, string[] members) : this(new ErrorData(error, members))
        {

        }

        public ValidationErrorException(string error) : this(new ErrorData(error))
        {

        }

        public ValidationErrorException(ErrorData error) : base(error?.ErrorMessage)
        {
            Error = error;
        }

        public ValidationErrorException(ErrorData error, Exception inner) : base(
            error?.ErrorMessage, inner)
        {
        }
        protected ValidationErrorException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
