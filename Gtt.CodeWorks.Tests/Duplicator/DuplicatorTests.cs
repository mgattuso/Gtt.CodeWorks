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
            mt.AddType(typeof(GenericBase));
            mt.AddType(typeof(Basic));
            var r = mt.Process();
            Console.WriteLine(r);
        }
    }

    public class GenericBase : ServiceBase<ReferenceClass>
    {
        public string  Message { get; set; }
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
}
