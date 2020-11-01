using System;
using System.Text;

namespace Gtt.CodeWorks
{
    [Serializable]
    public class ValidationErrorException : Exception
    {
        public Guid CorrelationId { get; }
        public ErrorData Error { get; }

        public ValidationErrorException(string error, string member, Guid correlationId) 
            : this(new ErrorData(error, member), correlationId)
        {
            CorrelationId = correlationId;
        }

        public ValidationErrorException(string error, string[] members, Guid correlationId) : this(new ErrorData(error, members), correlationId)
        {
            CorrelationId = correlationId;
        }

        public ValidationErrorException(string error, Guid correlationId) : this(new ErrorData(error), correlationId)
        {
            CorrelationId = correlationId;
        }

        public ValidationErrorException(ErrorData error, Guid correlationId) : base(error?.ErrorMessage)
        {
            Error = error;
            CorrelationId = correlationId;
        }

        public ValidationErrorException(ErrorData error, Exception inner, Guid correlationId) : base(
            error?.ErrorMessage, inner)
        {
            CorrelationId = correlationId;
        }
        protected ValidationErrorException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
