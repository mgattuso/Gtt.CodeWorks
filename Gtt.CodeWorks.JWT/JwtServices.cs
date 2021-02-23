using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Services;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using JWT.Serializers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Gtt.CodeWorks.JWT
{
    public class JwtServices : IUserResolver, IAuthTokenGenerator
    {
        private readonly IUserResolverSecret _userResolverSecret;
        private readonly ILogger<JwtServices> _logger;

        public JwtServices(IUserResolverSecret userResolverSecret, ILogger<JwtServices> logger)
        {
            _userResolverSecret = userResolverSecret;
            _logger = logger;
        }

        public Task<string> Generate(UserInformation user, Guid correlationId, TimeSpan? expiresIn, CancellationToken cancellationToken)
        {
            var token = new JwtBuilder()
                .WithAlgorithm(new HMACSHA512Algorithm())
                .WithSecret(_userResolverSecret.GetSecret())
                .AddClaim("sub", user.Username)
                .AddClaim("uid", user.UserIdentifier)
                .AddClaim("iat", ServiceClock.CurrentTime().ToUnixTimeSeconds())
                ;

            if (expiresIn != null)
            {
                token.AddClaim("exp", ServiceClock.CurrentTime().Add(expiresIn.Value).ToUnixTimeSeconds());
            }

            if (user.Roles != null && user.Roles.Length > 0)
            {
                token.AddClaim("roles", user.Roles);
            }
            else
            {
                token.AddClaim("roles", new string[0]);
            }

            if (user.Claims != null && user.Claims.Any())
            {
                foreach (var claim in user.Claims)
                {
                    string[] ignore = { "exp", "roles", "uid", "iat", "sub" };
                    if (!ignore.Contains(claim.Key))
                    {
                        token.AddClaim(claim.Key, claim.Value);
                    }
                }
            }

            var tokenStr = token.Encode();
            _logger.LogTrace($"Generated, token: {tokenStr}, correlationId: {correlationId}");
            return Task.FromResult(tokenStr);
        }

        public Task<UserResolverResult> GetUserOrDefault(string authToken, Guid correlationId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(authToken))
            {
                return Task.FromResult(new UserResolverResult(UserAuthStatus.NoUser));
            }

            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                var provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtAlgorithm algorithm = new HMACSHA512Algorithm(); // symmetric
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

                var userDict = decoder.DecodeToObject<Dictionary<string, object>>(authToken, _userResolverSecret.GetSecret(), verify: true);

                var user = new UserInformation
                {
                    Username = userDict.GetValueOrDefault("sub")?.ToString(),
                    Roles = ((JArray)userDict.GetValueOrDefault("roles")).Select(x => x.ToString()).ToArray(),
                    Claims = userDict,
                    UserIdentifier = userDict.GetValueOrDefault("uid")?.ToString(),
                    Expiration = userDict.ContainsKey("exp") ? DateTimeOffset.FromUnixTimeSeconds((long)userDict["exp"]) : (DateTimeOffset?)null,
                    AuthenticatedAt = DateTimeOffset.FromUnixTimeSeconds((long)userDict["iat"])
                };


                return Task.FromResult(new UserResolverResult(UserAuthStatus.Valid, user));
            }
            catch (TokenExpiredException)
            {
                _logger.LogInformation($"TokenExpired for CorrelationId:{correlationId}");
                return Task.FromResult(new UserResolverResult(UserAuthStatus.Expired));
            }
            catch (SignatureVerificationException ex)
            {
                _logger.LogError($"SignatureVerificationException for CorrelationId:{correlationId}", ex);
                return Task.FromResult(new UserResolverResult(UserAuthStatus.Invalid));
            }
            catch (Exception ex)
            {
                _logger.LogError($"General Exception for CorrelationId:{correlationId}", ex);
                return Task.FromResult(new UserResolverResult(UserAuthStatus.Invalid));
            }
        }
    }
}
