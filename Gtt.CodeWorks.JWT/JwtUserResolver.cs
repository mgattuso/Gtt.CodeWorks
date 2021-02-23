using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Services;
using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;
using Microsoft.Extensions.Logging;

namespace Gtt.CodeWorks.JWT
{
    public class JwtUserResolver : IUserResolver
    {
        private readonly IUserResolverSecret _userResolverSecret;
        private readonly ILogger<JwtUserResolver> _logger;

        public JwtUserResolver(IUserResolverSecret userResolverSecret, ILogger<JwtUserResolver> logger)
        {
            _userResolverSecret = userResolverSecret;
            _logger = logger;
        }
        public Task<UserResolverResult> GetUserOrDefault(string authToken, Guid correlationId, CancellationToken cancellationToken)
        {
            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                var provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtAlgorithm algorithm = new HMACSHA512Algorithm(); // symmetric
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

                var user = decoder.DecodeToObject<UserInformation>(authToken, _userResolverSecret.GetSecret(), verify: true);
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
