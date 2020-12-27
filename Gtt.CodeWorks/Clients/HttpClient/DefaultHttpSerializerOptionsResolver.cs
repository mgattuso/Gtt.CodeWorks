namespace Gtt.CodeWorks.Clients.HttpClient
{
    public class DefaultHttpSerializerOptionsResolver : IHttpSerializerOptionsResolver
    {
        public HttpDataSerializerOptions Options()
        {
            return new HttpDataSerializerOptions();
        }
    }
}