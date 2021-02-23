using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.JWT;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.JWT
{
    [TestClass]
    public class GenerateJwtTokenTest
    {
        [TestMethod]
        public async Task GenerateToken()
        {
            ServiceClock.CurrentTime = () => new DateTimeOffset(2020,01,01,1,0,0, TimeSpan.Zero);
            // ARRANGE
            var jwt = new JwtServices(new UserResolverSecretBasic("ABC"), NullLoggerFactory.Instance.CreateLogger<JwtServices>());
            var token = await jwt.Generate(new UserInformation
            {
                Username = "Test",
                UserIdentifier = "12345",
                Roles = new[] { "A" }
            }, Guid.Empty, null, CancellationToken.None);
            Assert.AreEqual("eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzUxMiJ9.eyJzdWIiOiJUZXN0IiwidWlkIjoiMTIzNDUiLCJpYXQiOjE1Nzc4NDA0MDAsInJvbGVzIjpbIkEiXX0.-cgfiJfrxO3u_7gfq1ErHLrDpQG8vwXGVw8BrBIxTZuNxdptzl08mxAsLlLql9Y7FLinSasYXKvQLvn7mrQWMw", token);
            ServiceClock.ResetToUtc();
        }

        [TestMethod]
        public async Task GenerateTokenAndBack()
        {
            ServiceClock.CurrentTime = () => new DateTimeOffset(2020, 01, 01, 1, 0, 0, TimeSpan.Zero);
            // ARRANGE
            var jwt = new JwtServices(new UserResolverSecretBasic("ABC"), NullLoggerFactory.Instance.CreateLogger<JwtServices>());
            var user = new UserInformation
            {
                Username = "Test",
                UserIdentifier = "12345",
                Roles = new[] {"A"}
            };
            var token = await jwt.Generate(user, Guid.Empty, null, CancellationToken.None);
            Assert.AreEqual("eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzUxMiJ9.eyJzdWIiOiJUZXN0IiwidWlkIjoiMTIzNDUiLCJpYXQiOjE1Nzc4NDA0MDAsInJvbGVzIjpbIkEiXX0.-cgfiJfrxO3u_7gfq1ErHLrDpQG8vwXGVw8BrBIxTZuNxdptzl08mxAsLlLql9Y7FLinSasYXKvQLvn7mrQWMw", token);
            ServiceClock.ResetToUtc();

            var userBack = await jwt.GetUserOrDefault(token, Guid.Empty, CancellationToken.None);
            Assert.AreEqual(user.Username, userBack.User.Username);
        }
    }
}
