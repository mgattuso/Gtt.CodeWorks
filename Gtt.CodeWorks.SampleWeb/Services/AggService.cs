//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Gtt.CodeWorks.Clients.Local;

//namespace Gtt.CodeWorks.SampleWeb.Services
//{
//    public class AggService : BaseServiceInstance<AggService.Request, AggService.Response>
//    {
//        private readonly ILocalService<GetAccountService> _accountService;
//        private readonly ILocalService<GetProfileService> _profileService;

//        public class Request : BaseRequest
//        {

//        }

//        public class Response
//        {
//            public string AccountName { get; set; }
//            public string Name { get; set; }
//        }

//        public AggService(
//            CoreDependencies coreDependencies,
//            ILocalService<GetAccountService> accountService,
//            ILocalService<GetProfileService> profileService) : base(coreDependencies)
//        {
//            _accountService = accountService;
//            _profileService = profileService;
//        }

//        protected override async Task<ServiceResponse<Response>> Implementation(Request request, CancellationToken cancellationToken)
//        {
//            var account = await _accountService.Call<IGetAccountService.Request, IGetAccountService.Response>(this,
//                new IGetAccountService.Request
//                {
//                    AccountId = 123
//                }, cancellationToken);

//            var profile = await _profileService.Call<IGetProfileService.Request, IGetProfileService.Response>(
//                this, new IGetProfileService.Request
//                {
//                    AccountId = 123
//                }, cancellationToken);

//            var response = new Response
//            {
//                AccountName = account.Data.Name,
//                Name = profile.Data.Name
//            };

//            return Successful(response);
//        }

//        protected override Task<string> CreateDistributedLockKey(Request request, CancellationToken cancellationToken)
//        {
//            return NoDistributedLock();
//        }

//        protected override IDictionary<int, string> DefineErrorCodes()
//        {
//            return NoErrorCodes();
//        }

//        public override ServiceAction Action => ServiceAction.Create;
//    }
//}
