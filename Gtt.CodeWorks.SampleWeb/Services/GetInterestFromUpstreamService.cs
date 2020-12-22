﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Web;

namespace Gtt.CodeWorks.SampleWeb.Services
{
    public class GetInterestFromUpstreamService : BaseServiceInstance<GetInterestFromUpstreamService.Request, GetInterestFromUpstreamService.Response>
    {
        private readonly IHttpDataSerializer _dataSerializer;
        private static readonly HttpClient Client = new HttpClient();

        public GetInterestFromUpstreamService(IHttpDataSerializer dataSerializer, CoreDependencies coreDependencies) : base(coreDependencies)
        {
            _dataSerializer = dataSerializer;
        }

        protected override Task<ServiceResponse<Response>> Implementation(Request request, CancellationToken cancellationToken)
        {
            var converter = new HttpClientConverter(Client, _dataSerializer);
            return converter.Call<Request, Response>(request, new Uri("https://gtt-global-financial.azurewebsites.net/api/call/CompoundInterestService"));
        }

        protected override Task<string> CreateDistributedLockKey(Request request, CancellationToken cancellationToken)
        {
            return Task.FromResult("");
        }

        public override ServiceAction Action => ServiceAction.Create;


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