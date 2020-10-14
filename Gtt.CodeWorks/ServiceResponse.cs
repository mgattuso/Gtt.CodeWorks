namespace Gtt.CodeWorks
{
    public class ServiceResponse<TResponse> : ServiceResponse where TResponse : new()
    {
        public ServiceResponse(TResponse data, ResponseMetaData metaData) : base(metaData)
        {
            Data = data;
        }

        public TResponse Data { get; }

    }

    public class ServiceResponse
    {
        public ServiceResponse(ResponseMetaData metaData)
        {
            MetaData = metaData;
        }

        public ResponseMetaData MetaData { get; }
    }
}