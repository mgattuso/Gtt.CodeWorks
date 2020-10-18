using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Gtt.CodeWorks.AspNet
{
    public class HttpRequestRunner
    {
        private readonly HttpRequestConverter _converter;

        public HttpRequestRunner(HttpRequestConverter converter)
        {
            _converter = converter;
        }

        public async Task Execute<TReq, TRes>(
            HttpContext context,
            BaseServiceInstance<TReq, TRes> service,
            CancellationToken cancellationToken
        )
            where TReq : BaseRequest, new()
            where TRes : new()
        {
            var input = await _converter.ConvertRequest<TReq>(context.Request);
            var output = await service.Execute(input, cancellationToken);
            await _converter.ConvertResponse(output, context.Response);
        }

        public async Task Execute(
            HttpContext context,
            IServiceInstance service,
            CancellationToken cancellationToken
        )
        {
            var input = await _converter.ConvertRequest(service.RequestType, context.Request);
            var output = await service.Execute(input, cancellationToken);
            await _converter.ConvertResponse(output, context.Response);
        }
    }
}