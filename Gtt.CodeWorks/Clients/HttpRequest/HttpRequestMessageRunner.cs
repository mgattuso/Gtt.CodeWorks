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
                var output = await service.Execute(input, ServiceClock.CurrentTime(), cancellationToken);
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
                            ServiceClock.CurrentTime(),
                            (long)(ServiceClock.CurrentTime() - start).TotalMilliseconds,
                            service.CorrelationId == default(Guid) ? Guid.Empty : service.CorrelationId,
                            ServiceResult.ValidationError,
                            ex.Errors
                        )
                    ),
                    service.ResponseType);

                return error;
            }
            catch (Exception ex)
            {
                var serialEx = ex as CodeWorksSerializationException;
                string originalPayload = serialEx?.RawData ?? "";
                var errors = new Dictionary<string, object>
                {
                    {"Error", new[] {ex.ToString()}}
                };
                if (!string.IsNullOrWhiteSpace(originalPayload))
                {
                    errors["payload"] = new[] { originalPayload };
                }

                var error = await _responseGenerator.ConvertResponse(
                    request,
                    new ServiceResponse(
                        new ResponseMetaData(
                            service.FullName,
                            ServiceClock.CurrentTime(),
                            (long)(ServiceClock.CurrentTime() - start).TotalMilliseconds,
                            service.CorrelationId == default(Guid) ? Guid.Empty : service.CorrelationId,
                            ServiceResult.PermanentError,
                            errors
                        )
                    ),
                    service.ResponseType);

                return error;
            }
        }

    }
}
