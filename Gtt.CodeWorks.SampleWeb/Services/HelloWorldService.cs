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

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }
        protected override Task<ServiceResponse<Response>> Implementation(Request request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Implementation(request));
        }

        private ServiceResponse<Response> Implementation(Request request)
        {
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
