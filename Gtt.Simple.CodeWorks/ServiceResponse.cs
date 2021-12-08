namespace Gtt.Simple.CodeWorks
{
    public class ServiceResponse
    {
        public ServiceResponse(ResponseMetaData metaData)
        {
            MetaData = metaData;
        }

        [AlwaysPresent]
        public ResponseMetaData MetaData { get; }
    }


    public class ServiceResponse<TResponse> : ServiceResponse where TResponse : class, new()
    {
        public ServiceResponse(TResponse data, ResponseMetaData metaData) : base(metaData)
        {
            Data = data;
        }

        public ServiceResponse(ResponseMetaData metaData) : base(metaData)
        {

        }

        public TResponse? Data { get; set; }
    }
}
