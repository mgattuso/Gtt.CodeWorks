using System;

namespace Gtt.CodeWorks
{
    public class BusinessLogicException : Exception
    {
        public ServiceResult Result { get; }
        public string PublicMessage { get; }

        public BusinessLogicException(string publicMessage, string message, ServiceResult result) : base(message)
        {
            PublicMessage = publicMessage;
            Result = result;
        }

        public BusinessLogicException(string publicMessage, string message, Exception ex) : base(message, ex)
        {
            PublicMessage = publicMessage;
        }
    }
}