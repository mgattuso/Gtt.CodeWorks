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

        public async Task<HttpResponseMessage> Handle(
            HttpRequestMessage request, 
            IServiceInstance service,
            CancellationToken cancellationToken)
        {
            var input = await _responseGenerator.ConvertRequest(service.RequestType, request);
            var output = await service.Execute(input, ServiceClock.CurrentTime(), cancellationToken);
            var response = await _responseGenerator.ConvertResponse(output, service.ResponseType);
            return response;
        }

    }
}
