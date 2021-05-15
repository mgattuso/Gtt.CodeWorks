using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.SampleWeb.Services
{
    public interface IGetProfileService : IServiceInstance<IGetProfileService.Request, IGetProfileService.Response>
    {
        public class Request : BaseRequest
        {
            public int AccountId { get; set; }
        }

        public class Response
        {
            public string Name { get; set; }
            public DateTime DateOfBirth { get; set; }
        }
    }

    public class GetProfileService : BaseServiceInstance<IGetProfileService.Request, IGetProfileService.Response>
    {
        public GetProfileService(CoreDependencies coreDependencies) : base(coreDependencies)
        {
        }

        protected override Task<ServiceResponse<IGetProfileService.Response>> Implementation(IGetProfileService.Request request, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Successful(new IGetProfileService.Response
                    {
                        Name = "Jane Doe",
                        DateOfBirth = new DateTime(2000, 2, 3)
                    }
                )
            );
        }

        protected override Task<string> CreateDistributedLockKey(IGetProfileService.Request request, CancellationToken cancellationToken)
        {
            return NoDistributedLock();
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }
    }
}
