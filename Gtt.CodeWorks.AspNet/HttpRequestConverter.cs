//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;

//namespace Gtt.CodeWorks.AspNet
//{
//    public class HttpRequestConverter
//    {
//        private readonly IHttpDataSerializer _serializer;

//        public HttpRequestConverter(IHttpDataSerializer serializer)
//        {
//            _serializer = serializer;
//        }

//        public async Task<T> ConvertRequest<T>(HttpRequest request) where T : BaseRequest
//        {
//            var contents = await request.Content.ReadAsStreamAsync();
//            var result = await _serializer.DeserializeRequest<T>(contents);
//            return result;
//        }

//        public async Task<HttpResponseMessage> ConvertResponse<T>(ServiceResponse<T> response) where T : new()
//        {
//            var serializedData = await _serializer.SerializeResponse(response);
//            var contentType = _serializer.ContentType;
//            var encoding = _serializer.Encoding;
//            var httpMsg = new HttpResponseMessage
//            {
//                StatusCode = (HttpStatusCode)response.MetaData.Result.HttpStatusCode(),
//                Content = new StringContent(serializedData, encoding, contentType)
//            };
//            httpMsg.Headers.Add(nameof(response.MetaData.CorrelationId), $"{response.MetaData.CorrelationId}");

//            return httpMsg;
//        }
//    }
//}
