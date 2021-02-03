using System;
using System.Collections.Generic;
using System.Text;
using Gtt.CodeWorks.Functions.Host;
using Gtt.CodeWorks.SampleServices;
using Gtt.CodeWorks.Serializers.TextJson;
using Gtt.CodeWorks.StateMachines;
using Gtt.CodeWorks.StateMachines.AzureStorage;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Gtt.CodeWorks.SampleAzureFunc.Startup))]

namespace Gtt.CodeWorks.SampleAzureFunc
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.ConfigureCodeWorksAll<TimeService>("Gtt.CodeWorks.SampleServices");
            builder.Services.AddSingleton<StatefulDependencies>();
            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddTransient<IObjectSerializer, JsonObjectSerializer>();
            builder.Services.AddTransient<IStateRepository>(cfg =>
                new AzureTableStateRepository(
                    "DefaultEndpointsProtocol=https;AccountName=gttglobalstorage;AccountKey=O8vyMv4OYSGIU1se/pNnNA+zRzbr/udAGczHtMOANJfr3JTDpv8T0PYhUBwNIm86/42m+xaJYUyzHnoFX883+g==;EndpointSuffix=core.windows.net",
                    cfg.GetService<IObjectSerializer>(),
                    cfg.GetService<ILogger<AzureTableStateRepository>>()
                    ));
        }
    }
}
