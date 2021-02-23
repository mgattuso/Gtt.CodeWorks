using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Services
{
    public class DummyUserResolver : IUserResolver
    {
        private readonly IServiceEnvironmentResolver _serviceEnvironmentResolver;

        public DummyUserResolver(IServiceEnvironmentResolver serviceEnvironmentResolver)
        {
            _serviceEnvironmentResolver = serviceEnvironmentResolver;
        }
        public Task<UserResolverResult> GetUserOrDefault(string authToken, Guid correlationId, CancellationToken cancellationToken)
        {
            if (_serviceEnvironmentResolver.Environment == CodeWorksEnvironment.Production)
            {
                throw new Exception("Cannot use the DummyUserResolver in production");
            }

            UserResolverResult result = new UserResolverResult(UserAuthStatus.NoUser);

            switch (authToken)
            {
                case "A":
                    result = new UserResolverResult(UserAuthStatus.Valid, new UserInformation
                    {
                        Username = "A",
                        UserIdentifier = "A",
                        Roles = new [] { "A" }
                    });
                    break;
                case "B":
                    result = new UserResolverResult(UserAuthStatus.Valid, new UserInformation
                    {
                        Username = "B",
                        UserIdentifier = "B",
                        Roles = new[] { "B" }
                    });
                    break;
                case "C":
                    result = new UserResolverResult(UserAuthStatus.Valid, new UserInformation
                    {
                        Username = "C",
                        UserIdentifier = "C",
                        Roles = null
                    });
                    break;
            }

            return Task.FromResult(result);
        }
    }
}