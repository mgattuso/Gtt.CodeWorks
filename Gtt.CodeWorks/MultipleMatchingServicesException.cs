using System;
using System.Runtime.Serialization;

namespace Gtt.CodeWorks
{
    [Serializable]
    public class MultipleMatchingServicesException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public MultipleMatchingServicesException()
        {
        }

        public MultipleMatchingServicesException(string message) : base(message)
        {
        }

        public MultipleMatchingServicesException(string message, Exception inner) : base(message, inner)
        {
        }

        protected MultipleMatchingServicesException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}