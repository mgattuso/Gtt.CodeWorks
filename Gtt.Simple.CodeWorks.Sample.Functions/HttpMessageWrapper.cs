using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.Simple.CodeWorks.Sample.Functions
{
    public class HttpMessageWrapper
    {
        private readonly ILogger logger;

        public HttpMessageWrapper(ILogger<HttpMessageWrapper> logger)
        {
            this.logger = logger;
        }

        public async Task<HttpResponseMessage> CallFromHttp<TRequest, TResponse>(IServiceInstance<TRequest, TResponse> service, HttpRequestMessage request, Action<TRequest, RequestMapping> mapping, CancellationToken cancellationToken)
            where TRequest : BaseRequest, new() where TResponse : class, new()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var requestJson = await request.Content.ReadAsStringAsync();
            var requestPayload = new TRequest();

            if (!string.IsNullOrWhiteSpace(requestJson))
            {
                requestPayload = JsonConvert.DeserializeObject<TRequest>(requestJson);
            }

            try
            {
                mapping(requestPayload, new RequestMapping(request));
            } catch (Exception ex)
            {
                logger.LogError(ex, "Cannot map object. Skipping the remainder of the mapping");
            }

            var responsePayload = await service.Execute(requestPayload, cancellationToken);
            var responseJson = JsonConvert.SerializeObject(responsePayload);
            var httpStatusCode = ServiceResultExpander.HttpStatusCode(responsePayload.MetaData.Result);

            var response = new HttpResponseMessage((HttpStatusCode)httpStatusCode)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            response.Headers.Add("x-cw-svc-duration", responsePayload.MetaData.DurationMs.ToString());
            response.Headers.Add("x-cw-correlationId", responsePayload.MetaData.CorrelationId.ToString());
            response.Headers.Add("x-cw-result", responsePayload.MetaData.Result.ToString());
            sw.Stop();
            response.Headers.Add("x-cw-e2e-duration", sw.ElapsedMilliseconds.ToString());
            return response;

        }


        public async Task<HttpResponseMessage> CallFromHttp(IServiceInstance service, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestJson = await request.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(requestJson))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            var requestPayload = JsonConvert.DeserializeObject(requestJson, service.RequestType) as BaseRequest;

            if (requestPayload == null)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            var responsePayload = await service.Execute(requestPayload, cancellationToken);
            var responseJson = JsonConvert.SerializeObject(responsePayload);
            var httpStatusCode = ServiceResultExpander.HttpStatusCode(responsePayload.MetaData.Result);

            var response = new HttpResponseMessage((HttpStatusCode)httpStatusCode)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            };

            response.Headers.Add("x-cw-duration", responsePayload.MetaData.DurationMs.ToString());
            response.Headers.Add("x-cw-correlationId", responsePayload.MetaData.CorrelationId.ToString());
            response.Headers.Add("x-cw-result", responsePayload.MetaData.Result.ToString());

            return response;
        }
    }

    public class RequestMapping
    {
        private HttpRequestMessage _request;

        public RequestMapping(HttpRequestMessage request)
        {
            _request = request;
        }
    }
}
