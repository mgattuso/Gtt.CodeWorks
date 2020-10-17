using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Web
{
    public class HttpRequestMessageRunner
    {
        private readonly HttpRequestMessageConverter _responseGenerator;

        public HttpRequestMessageRunner(HttpRequestMessageConverter responseGenerator)
        {
            _responseGenerator = responseGenerator;
        }

        public async Task<HttpResponseMessage> Handle<TReq, TRes>(
            HttpRequestMessage request, 
            BaseServiceInstance<TReq, TRes> service,
            CancellationToken cancellationToken) 
            where TReq : BaseRequest, new() where TRes : new()
        {
            var input = await _responseGenerator.ConvertRequest<TReq>(request);
            var output = await service.Execute(input, cancellationToken);
            var response = await _responseGenerator.ConvertResponse(output);
            return response;
        }

    }
}
