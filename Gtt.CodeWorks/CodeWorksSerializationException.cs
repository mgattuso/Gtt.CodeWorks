using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Gtt.CodeWorks
{
    [Serializable]
    public class CodeWorksSerializationException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public CodeWorksSerializationException()
        {
        }

        public CodeWorksSerializationException(string message) : base(message)
        {
        }

        public CodeWorksSerializationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected CodeWorksSerializationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
