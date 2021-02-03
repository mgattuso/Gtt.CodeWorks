using System;

namespace Gtt.CodeWorks
{
    public class BusinessLogicException : Exception
    {
        public ServiceResult Result { get; }

        public BusinessLogicException(string message, ServiceResult result) : base(message)
        {
            Result = result;
        }

        public BusinessLogicException(string message, Exception ex) : base(message, ex)
        {

        }
    }
}