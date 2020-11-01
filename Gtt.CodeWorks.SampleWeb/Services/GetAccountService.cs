using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.SampleWeb.Services
{
    public interface IGetAccountService : IServiceInstance<IGetAccountService.Request, IGetAccountService.Response>
    {
        public class Request : BaseRequest
        {
            public int AccountId { get; set; }
        }

        public class Response
        {
            public int AccountId { get; set; }
            public string Name { get; set; }
            public decimal Balance { get; set; }
        }
    }

    public class GetAccountService : 
        BaseReadService<IGetAccountService.Request, IGetAccountService.Response>,
        IGetAccountService
    {
        public GetAccountService(CoreDependencies coreDependencies) : base(coreDependencies)
        {
        }

        protected override Task<ServiceResponse<IGetAccountService.Response>> Implementation(IGetAccountService.Request request, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Successful(new IGetAccountService.Response
                {
                    AccountId = request.AccountId,
                    Balance = 100 * request.AccountId,
                    Name = $"Account {request.AccountId}"
                })
            );
        }
    }
}
