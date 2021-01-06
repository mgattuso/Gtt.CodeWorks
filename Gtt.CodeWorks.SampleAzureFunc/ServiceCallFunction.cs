using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Clients.HttpRequest;
using Gtt.CodeWorks.Functions.Host;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Gtt.CodeWorks.SampleAzureFunc
{
    public class ServiceCallFunction : ServiceCallFunctionBase
    {
        public ServiceCallFunction(
            HttpRequestMessageRunner runner, 
            IServiceResolver serviceResolver, 
            TelemetryClient telemetryClient) 
            : base(runner, serviceResolver, telemetryClient)
        {
        }

        [FunctionName("ServiceCallFunction")]
        public override Task<HttpResponseMessage> Execute(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{action}/{*service}")]
            HttpRequestMessage request,
            string action,
            string service,
            CancellationToken cancellationToken)
        {
            return base.Execute(request, action, service, cancellationToken);
        }
    }
}
