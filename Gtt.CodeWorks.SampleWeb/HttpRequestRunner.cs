using System;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Validation;
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
            await _converter.ConvertResponse(output, context.Response, new HttpDataSerializerOptions());
        }

        public async Task Execute(
            HttpContext context,
            IServiceInstance service,
            CancellationToken cancellationToken
        )
        {
            DateTimeOffset start = ServiceClock.CurrentTime();
            ServiceResponse output;
            Guid? correlationId = null;
            try
            {
                var input = await _converter.ConvertRequest(service.RequestType, context.Request, new HttpDataSerializerOptions());
                correlationId = input.CorrelationId;
                output = await service.Execute(input, start, cancellationToken);
            }
            catch (ValidationErrorException vex)
            {
                output = new ServiceResponse(
                    new ResponseMetaData(
                        service,
                        ServiceResult.ValidationError,
                        validationErrors: ValidationHelper.Create(vex.Error, vex.Member)));
            }

            await _converter.ConvertResponse(output, service.ResponseType, context.Response);
        }
    }
}