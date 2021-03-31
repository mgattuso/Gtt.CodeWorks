using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Gtt.CodeWorks.Tokenizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.Core.Tokenizer
{
    [TestClass]
    public class ObjectTokenizerTests
    {
        [TestMethod]
        public async Task TokenizeMask()
        {
            // arrange 
            var obj = new Object1
            {
                Ssn = "123456789"
            };

            await ObjectTokenizer.Tokenize(obj, new NullTokenService());

            // assert
            Assert.AreEqual("*****6789", obj.Ssn.MaskedValue);
        }

        [TestMethod]
        public async Task TokenizeMaskNestedObject()
        {
            // arrange 
            var obj = new Object1
            {
                O2 = new Object1.Object2
                {
                    Ssn = "123456789"
                }
            };

            await ObjectTokenizer.Tokenize(obj, new NullTokenService());

            // assert
            Assert.AreEqual("******789", obj.O2.Ssn.MaskedValue);
        }

        public class Object1
        {
            [Sensitive(Reveal.Last, lastChars: 4)]
            public TokenString Ssn { get; set; }
            public Object2 O2 { get; set; }

            public class Object2
            {
                [Sensitive(Reveal.Last, lastChars: 3)]
                public TokenString Ssn { get; set; }
            }
        }
    }
}
