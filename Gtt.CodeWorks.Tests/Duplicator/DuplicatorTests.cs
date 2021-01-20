using System;
using System.Collections.Generic;
using System.Text;
using Common.Models;
using Gtt.CodeWorks.Duplicator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.Duplicator
{
    [TestClass]
    public class ModelTreeTests
    {
        [TestMethod]
        public void BasicClassTest()
        {
            var mt = new Copier();
            //mt.LimitOutputToAssemblyOfType(typeof(Common.Models.Root));
            // mt.AddType(typeof(GenericBase));
            //mt.AddType(typeof(Basic));
            mt.AddType(typeof(ServiceResponse<CompoundResponse>));
            var r = mt.Process();
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
