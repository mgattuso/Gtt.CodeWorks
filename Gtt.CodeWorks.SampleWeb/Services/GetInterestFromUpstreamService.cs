using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Clients.HttpClient;
using Microsoft.Extensions.Logging;

namespace Gtt.CodeWorks.SampleWeb.Services
{
    public class GetInterestFromUpstreamService : BaseServiceInstance<GetInterestFromUpstreamService.Request, GetInterestFromUpstreamService.Response>
    {
        private readonly IHttpDataSerializer _dataSerializer;
        private readonly IHttpSerializerOptionsResolver _optionsResolver;
        private readonly ILoggerFactory _loggerFactory;
        private static readonly HttpClient Client = new HttpClient();

        public GetInterestFromUpstreamService(
            IHttpDataSerializer dataSerializer, 
            CoreDependencies coreDependencies,
            IHttpSerializerOptionsResolver optionsResolver,
            ILoggerFactory loggerFactory) : base(coreDependencies)
        {
            _dataSerializer = dataSerializer;
            _optionsResolver = optionsResolver;
            _loggerFactory = loggerFactory;
        }

        protected override Task<ServiceResponse<Response>> Implementation(Request request, CancellationToken cancellationToken)
        {
            var converter = new HttpClientConverter(Client, _dataSerializer, _optionsResolver, _loggerFactory.CreateLogger<HttpClientConverter>());
            return converter.Call<Request, Response>(request, new Uri("https://gtt-global-financial.azurewebsites.net/api/call/CompoundInterestService"), cancellationToken);
        }

        protected override Task<string> CreateDistributedLockKey(Request request, CancellationToken cancellationToken)
        {
            return Task.FromResult("");
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }
        public class Request : BaseRequest
        {
            public double Principal { get; set; }
            public double InterestRate { get; set; }
            public double TimePeriods { get; set; }
            public double TimesPerPeriod { get; set; }
            public bool IncludeSchedule { get; set; }
        }

        public class Response
        {
            public double Principal { get; set; }
            public double PrincipalAndInterest { get; set; }
            public double PeriodInterest { get; set; }
            public double CumulativeInterest { get; set; }
            public List<InterestPerPeriodData> InterestPerPeriod { get; set; } = new List<InterestPerPeriodData>();

            public class InterestPerPeriodData
            {
                public int Period { get; set; }
                public double Interest { get; set; }
                public double PrincipalAndInterest { get; set; }
            }
        }
    }
}
