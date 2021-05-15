using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Models;
using Gtt.CodeWorks;
using Gtt.CodeWorks.Duplicator;
using Gtt.Financial.Core.Account;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stateless;

namespace Gtt.CodeWorks.Tests.Duplicator
{
    [TestClass]
    public class CopierTests
    {
        [TestMethod]
        public void BasicClassTest()
        {
            var c = new Copier();
            //c.LimitOutputToAssemblyOfType(typeof(Common.Models.Root));
            c.AddType(typeof(GenericBase));
            c.AddType(typeof(Basic));
            c.AddType(typeof(ServiceResponse<CompoundResponse>));
            c.AddType(typeof(MockRequest));
            c.AddType(typeof(OtherMockRequest));
            c.AddType(typeof(MockResponse));
            var r = c.Process();
            Console.WriteLine(r);
        }

        [TestMethod]
        public void ClassWithEnumFromUnrelatedClass()
        {
            var c = new Copier();
            c.AddType(typeof(MockRequest));
            c.AddType(typeof(OtherMockRequest));
            c.AddType(typeof(MockResponse));
            var r = c.Process();
            Console.WriteLine(r);
        }

        [TestMethod]
        public void ServiceTest()
        {
            var settings = new CopierSettings();
            settings.BaseTypesToRemove.Add(typeof(BaseServiceInstance<,>));
            var c = new Copier(settings);
            c.LimitOutputToAssemblyOfType(typeof(TokenRequest));
            c.AddType(typeof(TokenRequest));
            var r = c.Process();
            Console.WriteLine(r);
        }

        [TestMethod]
        public void ServiceTestWithReplacementType()
        {
            var settings = new CopierSettings
            {
                ReplacementTypes =
                {
                    { typeof(TokenDate), typeof(DateTime) },
                    { typeof(TokenString), typeof(string) },
                }
            };
            settings.BaseTypesToRemove.Add(typeof(BaseServiceInstance<,>));
            var c = new Copier(settings);
            c.LimitOutputToAssemblyOfType(typeof(TokenRequest));
            c.AddType(typeof(TokenRequest));
            var r = c.Process();
            Console.WriteLine(r);
        }

        [TestMethod]
        public void TokenTest()
        {
            var settings = new CopierSettings();
            settings.BaseTypesToRemove.Add(typeof(BaseServiceInstance<,>));
            var c = new Copier(settings);
            c.LimitOutputToAssemblyOfType(typeof(AccountRequest));
            c.AddType(typeof(AccountRequest));
            c.AddType(typeof(AccountResponse));
            var r = c.Process();
            Console.WriteLine(r);
        }
    }

    public class GenericBase : ServiceBase<ReferenceClass>
    {
        public string Message { get; set; }
    }

    public class Basic : Root
    {
        public int A { get; set; }
        public int? B { get; set; }
        public ReferenceClass Ref { get; set; }
        public List<ReferenceClass> Refs { get; set; }
        public Scenes Scene { get; set; }
    }
    public enum Scenes
    {
        Beach,
        Coastal,
        Inland,
        Mountains
    }

    public class MockService
    {
        public enum MockAction
        {
            Open,
            Close
        }

        public enum MockState
        {
            Opened,
            Closed
        }
    }

    public class MockRequest : BaseRequest
    {
        public MockService.MockAction Action { get; set; }
    }

    public class OtherMockRequest
    {
        public MockService.MockAction Action { get; set; }
    }

    public class MockResponse
    {
        public MockService.MockState State { get; set; }
    }
}

namespace Common.Models
{
    public class ReferenceClass
    {
        public DateTimeOffset Create { get; set; }
    }

    public abstract class Root
    {
        public Guid CorrelationId { get; set; }
    }

    public class ServiceBase<T> where T : ReferenceClass
    {
        public T Instance { get; set; }
    }

    public class CompoundResponse
    {
        public double Principal { get; set; }
        public double PrincipalAndInterestExact { get; set; }
        public double PrincipalAndInterestCurrency { get; set; }
        public double InterestExact { get; set; }
        public double InterestCurrency { get; set; }
        public List<InterestPerPeriodData> InterestPerPeriod { get; set; } = new List<InterestPerPeriodData>();

        public class InterestPerPeriodData
        {
            public int Period { get; set; }
            public double PeriodInterestExact { get; set; }
            public double PeriodInterestCurrency { get; set; }
            public double CumulativeInterestExact { get; set; }
            public double CumulativeInterestCurrency { get; set; }
            public double PrincipalAndInterestExact { get; set; }
            public double PrincipalAndInterestCurrency { get; set; }
            public object SomeObject { get; set; }
        }
    }
}

namespace Gtt.Financial.Core.Account
{
    public class AccountService : BaseServiceInstance<AccountRequest, AccountResponse>
    {
        public AccountService(CoreDependencies coreDependencies) : base(coreDependencies)
        {
        }

        protected override Task<ServiceResponse<AccountResponse>> Implementation(AccountRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task<string> CreateDistributedLockKey(AccountRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.AccountIdentifier.ToString());
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }

        private StateMachine<AccountState, AccountAction> _stateMachine =
            new StateMachine<AccountState, AccountAction>(AccountState.Active);

        public enum AccountState
        {
            Active,
            Closed
        }
        public enum AccountAction
        {
            Open,
            Close
        }
    }

    public class AccountRequest : BaseRequest
    {
        public Guid AccountIdentifier { get; set; }
        public AccountService.AccountAction Action { get; set; }

        public OpenData Open { get; set; }
        public CloseData Close { get; set; }

        public class OpenData
        {
            public decimal InitialBalance { get; set; }
        }

        public class CloseData
        {

        }
    }

    public class AccountResponse
    {
        public Guid AccountIdentifier { get; set; }
        public AccountService.AccountState State { get; set; }
        public AccountService.AccountAction[] Actions { get; set; }
        public List<AccountService.AccountAction> ListOfActions { get; set; }
    }

    public class TokenRequest
    {
        public TokenString TokenString { get; set; }
        public TokenDate TokenDate { get; set; }
    }
}
