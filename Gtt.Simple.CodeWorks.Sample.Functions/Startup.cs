using Gtt.Simple.CodeWorks.SampleService;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(Gtt.Simple.CodeWorks.Sample.Functions.Startup))]

namespace Gtt.Simple.CodeWorks.Sample.Functions
{

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();
            builder.Services.AddTransient<StopwatchService>();
            builder.Services.AddTransient<HttpMessageWrapper>();
            builder.Services.AddTransient<BalanceService>();
        }
    }

    public class StopwatchService
    {
        public StopwatchService()
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public Stopwatch Stopwatch { get; }
    }
}
