using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Gtt.CodeWorks
{
    [Serializable]
    public class SchemaValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public SchemaValidationException()
        {
        }

        public SchemaValidationException(string message, IDictionary<string, string[]> errors) : base(message)
        {
            Errors = errors;
        }

        public SchemaValidationException(string message) : base(message)
        {
        }

        public SchemaValidationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SchemaValidationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
