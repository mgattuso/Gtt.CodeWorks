using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Gtt.CodeWorks.EasyNetQ;
using Gtt.CodeWorks.Serializers.TextJson;
using Gtt.CodeWorks.Validation;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.Core
{
    [TestClass]
    public class BasicServiceTest
    {
        [TestMethod]
        public async Task StandardResponse()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 100000; i++)
            {
                var s = new TimeService(CoreDependencies.NullDependencies);
                var r = await s.Execute(new TimeService.Request(), ServiceClock.CurrentTime(), CancellationToken.None);
                Console.WriteLine(r.MetaData.DurationMs);
            }
            sw.Stop();
            Console.WriteLine($"Duration: {sw.ElapsedMilliseconds}");
        }

        [TestMethod]
        public async Task RabbitMqLogging()
        {
            using var bus = RabbitHutch.CreateBus("host=localhost");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var coreDependencies = new CoreDependencies(
                NullLoggerFactory.Instance, 
                new EasyNetQServiceLogger(new JsonLogObjectSerializer(), bus),
                NullTokenizer.SkipTokenization,
                new InMemoryRateLimiter(), 
                new InMemoryDistributedLock(), 
                new NonProductionEnvironmentResolver(),
                NullRequestValidator.Instance);

            Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = 8 }, async i =>
            {
                var s = new TimeService(coreDependencies);
                var r = await s.Execute(new TimeService.Request(), ServiceClock.CurrentTime(), CancellationToken.None);
            });
            sw.Stop();
            await Task.CompletedTask;
            Console.WriteLine($"Duration: {sw.ElapsedMilliseconds}");
        }

        public class TimeService : BaseReadService<TimeService.Request, TimeService.Response>
        {
            public class Response
            {
                public DateTimeOffset CurrentTime { get; set; }
            }

            public class Request : BaseRequest
            {
            }

            public TimeService(CoreDependencies coreDependencies) : base(coreDependencies)
            {
            }

            protected override async Task<ServiceResponse<Response>> Implementation(Request request, CancellationToken cancellationToken)
            {
                await Task.CompletedTask;

                var response = new Response
                {
                    CurrentTime = ServiceClock.CurrentTime()
                };

                return Successful(response);
            }
        }
    }
}
