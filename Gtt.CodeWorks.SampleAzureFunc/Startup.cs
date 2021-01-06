using System;
using System.Collections.Generic;
using System.Text;
using Gtt.CodeWorks.Functions.Host;
using Gtt.CodeWorks.SampleServices;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
[assembly: FunctionsStartup(typeof(Gtt.CodeWorks.SampleAzureFunc.Startup))]

namespace Gtt.CodeWorks.SampleAzureFunc
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.ConfigureCodeWorksAll<TimeService>("Gtt.CodeWorks.SampleServices");
            builder.Services.AddApplicationInsightsTelemetry();
        }
    }
}
