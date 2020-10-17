using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.Core
{
    [TestClass]
    public class BaseRequestExtensionsTests
    {
        [TestMethod]
        public void SyncCorrelationId_OneLevel()
        {
            // ARRANGE
            var root = new BaseRequestRoot();

            // ACT
            root.SyncCorrelationIds();

            // ASSERT
            Assert.AreEqual(root.CorrelationId, root.P2.CorrelationId);
            Assert.AreEqual(root.CorrelationId, root.P3.CorrelationId);
        }

        [TestMethod]
        public void SyncCorrelationId_TwoLevels()
        {
            // ARRANGE
            var root = new BaseRequestRoot();

            // ACT
            root.SyncCorrelationIds();

            // ASSERT
            Assert.AreEqual(root.CorrelationId, root.P2.CorrelationId);
            Assert.AreEqual(root.CorrelationId, root.P3.CorrelationId);
            Assert.AreEqual(root.CorrelationId, root.P2.P4.CorrelationId);
            Assert.AreEqual(root.CorrelationId, root.P2.P5.CorrelationId);
            Assert.AreEqual(root.CorrelationId, root.P3.P4.CorrelationId);
            Assert.AreEqual(root.CorrelationId, root.P3.P5.CorrelationId);
        }

        [TestMethod]
        public void SyncCorrelationId_Collections()
        {
            // ARRANGE
            var root = new BaseRequestRoot();

            // ACT
            root.SyncCorrelationIds();

            // ASSERT
            foreach (var r in root.C1)
            {
                Assert.AreEqual(root.CorrelationId, r.CorrelationId);
            }

            // ASSERT
            foreach (var r in root.C2)
            {
                Assert.AreEqual(root.CorrelationId, r.CorrelationId);
            }
        }

        [TestMethod]
        public void SyncCorrelationId_Null()
        {
            // ARRANGE
            var root = new BaseRequestRoot { P2 = null };

            // ACT
            root.SyncCorrelationIds();

            // ASSERT
            Assert.AreEqual(root.CorrelationId, root.P3.CorrelationId);
            Assert.IsNull(root.P2);
        }

        [TestMethod]
        public void SyncCorrelationId_EmptyCollection()
        {
            // ARRANGE
            var root = new BaseRequestRoot
            {
                C2 = null
            };

            // ACT
            root.SyncCorrelationIds();

            // ASSERT
            foreach (var r in root.C1)
            {
                Assert.AreEqual(root.CorrelationId, r.CorrelationId);
            }

            Assert.IsNull(root.C2);
        }

        [TestMethod]
        public void SyncCorrelationId_Dictionary()
        {
            // ARRANGE
            var root = new BaseRequestRoot();

            // ACT
            root.SyncCorrelationIds();

            // ASSERT
            foreach (var r in root.D1)
            {
                Assert.AreEqual(root.CorrelationId, r.Value.CorrelationId);
            }
        }

        [TestMethod]
        public void SyncCorrelationId_CircularReferences()
        {
            // ARRANGE
            var root = new BaseRequestCircularParent();

            // ACT
            root.SyncCorrelationIds();

            // ASSERT
            Assert.AreEqual(root.CorrelationId, root.Children.Skip(0).First().CorrelationId);
            Assert.AreEqual(root.CorrelationId, root.Children.Skip(1).First().CorrelationId);
            Assert.AreEqual(root.CorrelationId, root.Children.Skip(0).First().Parent.CorrelationId);
            Assert.AreEqual(root.CorrelationId, root.Children.Skip(1).First().Parent.CorrelationId);
        }

        public class BaseRequestRoot : BaseRequest
        {
            public string P1 { get; set; }
            public BaseRequestLevel1 P2 { get; set; } = new BaseRequestLevel1();
            public BaseRequestLevel1 P3 { get; set; } = new BaseRequestLevel1();
            public List<BaseRequestLevel1> C1 { get; set; } = new List<BaseRequestLevel1>(new[] { new BaseRequestLevel1(), new BaseRequestLevel1() });
            public BaseRequestLevel1[] C2 { get; set; } = { new BaseRequestLevel1(), new BaseRequestLevel1() };

            public IDictionary<string, BaseRequestLevel1> D1 { get; set; } = new Dictionary<string, BaseRequestLevel1>
            {
                ["A"] = new BaseRequestLevel1(),
                ["B"] = new BaseRequestLevel1()
            };

        }

        public class BaseRequestLevel1 : BaseRequest
        {
            public int P1 { get; set; }
            public string P2 { get; set; }
            public DateTime P3 { get; set; }
            public BaseRequestLevel2 P4 { get; set; } = new BaseRequestLevel2();
            public BaseRequestLevel2 P5 { get; set; } = new BaseRequestLevel2();
            public BaseRequestLevel2[] C1 { get; set; } = { new BaseRequestLevel2(), new BaseRequestLevel2() };
            public BaseRequestLevel2[] C2 { get; set; } = { new BaseRequestLevel2(), new BaseRequestLevel2() };

        }

        public class BaseRequestLevel2 : BaseRequest
        {

        }

        public class BaseRequestCircularParent : BaseRequest
        {
            public BaseRequestCircularParent()
            {
                Children = new List<BaseRequestCircularChild>();
                var child1 = new BaseRequestCircularChild(this);
                var child2 = new BaseRequestCircularChild(this);
            }

            public List<BaseRequestCircularChild> Children { get; set; }
        }

        public class BaseRequestCircularChild : BaseRequest
        {
            public BaseRequestCircularChild(BaseRequestCircularParent parent)
            {
                Parent = parent;
                parent.Children.Add(this);
            }

            public BaseRequestCircularParent Parent { get; set; }
        }
    }
}
