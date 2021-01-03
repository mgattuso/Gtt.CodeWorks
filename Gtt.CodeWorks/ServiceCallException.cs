using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Gtt.CodeWorks
{
    [Serializable]
    public class ServiceCallException : Exception
    {
        public ResponseMetaData MetaData { get; }
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ServiceCallException(ResponseMetaData metaData) : base($"Unsuccessful response received. {metaData.Result} from {metaData.ServiceName}")
        {
            MetaData = metaData;
        }

        protected ServiceCallException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
