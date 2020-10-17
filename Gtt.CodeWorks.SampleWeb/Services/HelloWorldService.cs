using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.SampleWeb.Services
{
    public class HelloWorldService : BaseServiceInstance<HelloWorldService.Request, HelloWorldService.Response>
    {
        public HelloWorldService(CoreDependencies coreDependencies) : base(coreDependencies)
        {
        }

        protected override Task<string> CreateDistributedLockKey(Request request, CancellationToken cancellationToken)
        {
            return Task.FromResult("ABC");
        }

        protected override async Task<ServiceResponse<Response>> Implementation(Request request, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            var response = new Response
            {
                Message = request.Message
            };

            if (string.IsNullOrEmpty(response.Message))
            {
                response.Message = "Hello World!";
            }

            return Successful(response);
        }

        public class Request : BaseRequest
        {
            public string Message { get; set; }
        }

        public class Response
        {
            public string Message { get; set; }
        }
    }
}
