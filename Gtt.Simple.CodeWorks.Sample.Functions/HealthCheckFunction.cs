using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Gtt.Simple.CodeWorks.SampleService;
using System.Threading;
using System.Net.Http;

namespace Gtt.Simple.CodeWorks.Sample.Functions
{
    public class HealthCheckFunction
    {
        private readonly BalanceService balanceService;
        private readonly HttpMessageWrapper httpMessageWrapper;
        private readonly StopwatchService stopwatchService;

        public HealthCheckFunction(BalanceService balanceService, HttpMessageWrapper httpMessageWrapper, StopwatchService stopwatchService)
        {
            this.balanceService = balanceService;
            this.httpMessageWrapper = httpMessageWrapper;
            this.stopwatchService = stopwatchService;
        }

        [FunctionName("HealthCheckFunction")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "account/{accountId}/balance")] HttpRequestMessage req, string accountId, CancellationToken cancellationToken)
        {
            var response = await httpMessageWrapper.CallFromHttp(balanceService, req, (m,r) => {
                m.AccountIdentifier = accountId;
            },  cancellationToken);

            response.Headers.Add("x-cw-http-duration", stopwatchService.Stopwatch.ElapsedMilliseconds.ToString());
            return response;
        }
    }
}
