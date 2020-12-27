using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.SampleWeb.Services
{
    public class GetInformationService : BaseServiceInstance<GetInformationService.Request, GetInformationService.Response>
    {
        private readonly ServiceDirectory _directory;

        public GetInformationService(CoreDependencies coreDependencies, ServiceDirectory directory) : base(coreDependencies)
        {
            _directory = directory;
        }

        protected override async Task<ServiceResponse<Response>> Implementation(Request request, CancellationToken cancellationToken)
        {
            var response = new Response();
            var accountInfo = await _directory.Call(new IGetAccountService.Request
            {
                AccountId = request.AccountId,
                CorrelationId = request.CorrelationId,
                ServiceHop = request.ServiceHop.GetValueOrDefault() + 1
            }, cancellationToken);
            var profileInfo = await _directory.Call(new IGetProfileService.Request
            {
                AccountId = request.AccountId,
                CorrelationId = request.CorrelationId,
                ServiceHop = request.ServiceHop.GetValueOrDefault() + 1
            }, cancellationToken);

            response.Account = accountInfo.Data;
            response.Profile = profileInfo.Data;
            return Successful(response, dependencyMetaData: new Dictionary<string, ResponseMetaData>
            {
                { accountInfo.MetaData.ServiceName, accountInfo.MetaData },
                { profileInfo.MetaData.ServiceName, profileInfo.MetaData }
            });
        }

        protected override Task<string> CreateDistributedLockKey(Request request, CancellationToken cancellationToken)
        {
            return Task.FromResult(string.Empty);
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }

        public override ServiceAction Action => ServiceAction.Read;

        public class Request : BaseRequest
        {
            public int AccountId { get; set; }
        }

        public class Response
        {
            public IGetAccountService.Response Account { get; set; }
            public IGetProfileService.Response Profile { get; set; }
        }
    }
}
