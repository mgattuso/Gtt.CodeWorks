namespace Gtt.CodeWorks
{
    public class ServiceResponse<TResponse> : ServiceResponse where TResponse : new()
    {
        public ServiceResponse(TResponse data, ResponseMetaData metaData) : base(metaData)
        {
            Data = data;
        }

        public TResponse Data { get; set; }
    }

    public class ServiceResponse
    {
        public ServiceResponse(ResponseMetaData metaData)
        {
            MetaData = metaData;
        }

        [AlwaysPresent]
        public ResponseMetaData MetaData { get; }
    }
}