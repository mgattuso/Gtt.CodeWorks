using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.Simple.CodeWorks.SampleService
{
    public class BalanceService : BaseServiceInstance<BalanceServiceRequest, BalanceServiceResponse>
    {

        public BalanceService(ILogger<BalanceService> logger) : base(logger)
        {

        }

        public async override Task<ServiceResponse<BalanceServiceResponse>> Run(BalanceServiceRequest request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var result = new BalanceServiceResponse
            {
                AccountIdentifier = request.AccountIdentifier,
                AvailableBalance = 101.23m,
                LedgerBalance = 178.45m
            };

            return Successful(result);
        }
    }

    public class BalanceServiceRequest : BaseRequest
    {
        public string? AccountIdentifier { get; set; }
    }

    public class BalanceServiceResponse
    {
        public string? AccountIdentifier { get; set; }
        public decimal? AvailableBalance { get; set; }
        public decimal? LedgerBalance { get; set; }
    }

}
