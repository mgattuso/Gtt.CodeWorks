using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Clients.HttpRequest;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Gtt.CodeWorks.Functions.Host
{
    public abstract class ServiceCallFunctionBase
    {
        private readonly HttpRequestMessageRunner _runner;
        private readonly IServiceResolver _serviceResolver;
        private readonly TelemetryClient _telemetryClient;

        protected ServiceCallFunctionBase(
            HttpRequestMessageRunner runner,
            IServiceResolver serviceResolver,
            TelemetryClient telemetryClient)
        {
            _runner = runner;
            _serviceResolver = serviceResolver;
            _telemetryClient = telemetryClient;
        }

        public virtual Task<HttpResponseMessage> Execute(
            HttpRequestMessage request,
            string action,
            string service,
            CancellationToken cancellationToken)
        {
            switch (action)
            {
                case "call":
                    return CallAction(request, service, cancellationToken);
            }

            throw new NotImplementedException();
        }

        protected async Task<HttpResponseMessage> CallAction(
            HttpRequestMessage request,
            string service,
            CancellationToken cancellationToken)
        {
            var dict = new Dictionary<string, string> { ["Service"] = service };
            var serviceInstance = _serviceResolver.GetInstanceByName(service);
            if (serviceInstance == null)
            {
                dict["Found"] = "false";
                _telemetryClient.TrackTrace("ServiceCall", SeverityLevel.Information, dict);
                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent($"No service found called {service}")
                };
            }
            dict["Found"] = "true";
            dict["ServiceInstance"] = serviceInstance.Name;
            _telemetryClient.TrackTrace("ServiceCall", SeverityLevel.Information, dict);
            var result = await _runner.Handle(request, serviceInstance, cancellationToken);
            return result;
        }
    }
}
