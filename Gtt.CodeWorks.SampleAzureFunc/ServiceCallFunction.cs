﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Clients.HttpRequest;
using Gtt.CodeWorks.Functions.Host;
using Gtt.CodeWorks.Services;
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
            IHttpDataSerializer serializer,
            ISerializationSchema serializationSchema,
            IStateDiagram stateDiagram,
            IChainedServiceResolver chainedServiceResolver,
            TelemetryClient telemetryClient)
            : base(runner, serviceResolver, serializer, serializationSchema, chainedServiceResolver, stateDiagram, telemetryClient)
        {
        }

        [FunctionName("ServiceCallFunction")]
        public override Task<HttpResponseMessage> Execute(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "get", Route = "{action}/{*service}")]
            HttpRequestMessage request,
            string action,
            string service,
            CancellationToken cancellationToken)
        {
            return base.Execute(request, action, service, cancellationToken);
        }
    }
}
