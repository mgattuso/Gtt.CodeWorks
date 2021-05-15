using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Gtt.CodeWorks.SampleWeb.Services
{
    public class ServiceDirectory 
        : ICallService<IGetAccountService.Request, IGetAccountService.Response>
        , ICallService<IGetProfileService.Request, IGetProfileService.Response>
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceDirectory(IServiceProvider serviceProvider
        )
        {
            _serviceProvider = serviceProvider;
        }

        public Task<ServiceResponse<IGetAccountService.Response>> Call(IGetAccountService.Request request, CancellationToken cancellationToken)
        {
            return _serviceProvider.GetService<IGetAccountService>()
                                   .Execute(request, cancellationToken);
        }

        public Task<ServiceResponse<IGetProfileService.Response>> Call(IGetProfileService.Request request, CancellationToken cancellationToken)
        {
            return _serviceProvider.GetService<IGetProfileService>()
                .Execute(request, cancellationToken);
        }
    }



    public interface ICallService<in TReq, TRes> where TReq : BaseRequest, new()
        where TRes : new()
    {
        Task<ServiceResponse<TRes>> Call(TReq request, CancellationToken cancellationToken);
    }
}
