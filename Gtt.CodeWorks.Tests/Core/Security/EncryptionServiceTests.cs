using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Gtt.CodeWorks.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.Core.Security
{
    [TestClass]
    public class EncryptionServiceTests
    {
        [TestMethod]
        public void Hash256()
        {
            string eKey = "EK1vgJyLaxeXoHDuS/jYo+tbmmY0hDeB1lPy9DKipWU=";
            string hKey = "g09SiTE7Jy4P6mimfOLRxtNXbjXbjOIfQrvYZCpwTTon8UNR4nnk6oi9TTHhqeFmrTVvNDtwARC1RQaJs5e/8g==";
            var es = new EncryptionService(eKey, hKey);
            var h = es.Hash(EncryptionStrength.Sha256, "a", caseInsensitive: false);
            Assert.AreEqual("dc0f3f501f8db404b657d17072dfa8b4e66643629a7435201775a2a1b6855f88", h);
        }

        [TestMethod]
        public void Hash512()
        {
            string eKey = "EK1vgJyLaxeXoHDuS/jYo+tbmmY0hDeB1lPy9DKipWU=";
            string hKey = "g09SiTE7Jy4P6mimfOLRxtNXbjXbjOIfQrvYZCpwTTon8UNR4nnk6oi9TTHhqeFmrTVvNDtwARC1RQaJs5e/8g==";
            var es = new EncryptionService(eKey, hKey);
            var h = es.Hash(EncryptionStrength.Sha512, "a", caseInsensitive: false);
            Assert.AreEqual("371aff6daad78a96595311c310268fc03c8fa768fefffbc1c4106eb5337301452b28ad437b0405c2afafeb8017df9902ed50f75838a1b34e3e7eb79069b2e29c", h);
        }

        [TestMethod]
        public void Encrypt256()
        {
            string eKey = "EK1vgJyLaxeXoHDuS/jYo+tbmmY0hDeB1lPy9DKipWU=";
            string hKey = "g09SiTE7Jy4P6mimfOLRxtNXbjXbjOIfQrvYZCpwTTon8UNR4nnk6oi9TTHhqeFmrTVvNDtwARC1RQaJs5e/8g==";
            var es = new EncryptionService(eKey, hKey);

            var e = es.Encrypt(EncryptionStrength.Sha256, "a");
            var d = es.Decrypt(e.Encrypted);
            Console.WriteLine(e.Encrypted);
            Assert.AreEqual("a", d);
        }

        [TestMethod]
        public void Encrypt512()
        {
            string eKey = "EK1vgJyLaxeXoHDuS/jYo+tbmmY0hDeB1lPy9DKipWU=";
            string hKey = "g09SiTE7Jy4P6mimfOLRxtNXbjXbjOIfQrvYZCpwTTon8UNR4nnk6oi9TTHhqeFmrTVvNDtwARC1RQaJs5e/8g==";
            var es = new EncryptionService(eKey, hKey);

            var e = es.Encrypt(EncryptionStrength.Sha512, "a");
            var d = es.Decrypt(e.Encrypted);
            Console.WriteLine(e.Encrypted);
            Assert.AreEqual("a", d);
        }
    }
}
