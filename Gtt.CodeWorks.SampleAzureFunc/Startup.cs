using System;
using System.Collections.Generic;
using System.Text;
using Gtt.CodeWorks.Functions.Host;
using Gtt.CodeWorks.JWT;
using Gtt.CodeWorks.SampleServices;
using Gtt.CodeWorks.Serializers.TextJson;
using Gtt.CodeWorks.Services;
using Gtt.CodeWorks.StateMachines;
using Gtt.CodeWorks.StateMachines.AzureStorage;
using Gtt.CodeWorks.Tokenizer;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Tokenize.Client;

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
            builder.Services.AddTransient<IObjectSerializer>(cfg => new JsonObjectSerializer(false));

            builder.Services.Replace(ServiceDescriptor.Transient<ITokenizerService, GttTokenizerService>());
            builder.Services.Replace(ServiceDescriptor.Transient<IStateDiagram, StatelessStateDiagram>());

            builder.Services.AddTransient<IUserResolver, DummyUserResolver>();
            builder.Services.AddTransient<IUserResolverSecret>(cfg => new UserResolverSecretBasic("ABC123"));

            builder.Services.AddTransient<ITokenizeClient>(cfg => new TokenizeClient(
                Environment.GetEnvironmentVariable("TokenizerEndpoint"),
                "codeworks",
                Environment.GetEnvironmentVariable("TokenizerKey")));
            builder.Services.AddTransient<IStateRepository>(cfg =>
                new AzureTableStateRepository(
                    Environment.GetEnvironmentVariable("StateRepository"),
                    cfg.GetService<IObjectSerializer>(),
                    cfg.GetService<ILogger<AzureTableStateRepository>>()
                    ));
        }
    }
}
