using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks
{
    public class BaseResponse<TResponse> : BaseResponse where TResponse : new()
    {
        public BaseResponse(TResponse data, ResponseMetaData metaData) : base(metaData)
        {
            Data = data;
        }

        public TResponse Data { get; }

    }

    public class BaseResponse 
    {
        public BaseResponse(ResponseMetaData metaData)
        {
            MetaData = metaData;
        }

        public ResponseMetaData MetaData { get; }
    }
}
