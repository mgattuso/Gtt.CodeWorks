using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Clients.HttpRequest
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
            var start = ServiceClock.CurrentTime();
            try
            {
                var input = await _responseGenerator.ConvertRequest(service.RequestType, request);
                var output = await service.Execute(input, cancellationToken);
                var response = await _responseGenerator.ConvertResponse(request, output, service.ResponseType);
                return response;
            }
            catch (SchemaValidationException ex)
            {
                var error = await _responseGenerator.ConvertResponse(
                    request,
                    new ServiceResponse(
                        new ResponseMetaData(
                            service.FullName,
                            service.CorrelationId == default(Guid) ? Guid.Empty : service.CorrelationId,
                            ServiceResult.ValidationError,
                            (long)(ServiceClock.CurrentTime() - start).TotalMilliseconds,
                            ServiceClock.CurrentTime(),
                            validationErrors: ex.Errors
                        )
                    ),
                    service.ResponseType);

                return error;
            }
            catch (Exception ex)
            {
                var serialEx = ex as CodeWorksSerializationException;
                string originalPayload = serialEx?.RawData ?? "";

                Dictionary<string, string[]> errors = null;
                if (!string.IsNullOrWhiteSpace(originalPayload))
                {
                    errors = new Dictionary<string, string[]> { ["payload"] = new[] { originalPayload } };
                }

                var error = await _responseGenerator.ConvertResponse(
                    request,
                    new ServiceResponse(
                        new ResponseMetaData(
                            service.FullName,
                            service.CorrelationId == default(Guid) ? Guid.Empty : service.CorrelationId,
                            ServiceResult.PermanentError,
                            (long)(ServiceClock.CurrentTime() - start).TotalMilliseconds,
                            ServiceClock.CurrentTime(),
                            validationErrors: errors,
                            exceptionMessage: ex.ToString()
                        )
                    ),
                    service.ResponseType);

                return error;
            }
        }

    }
}
