using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Services;
using Gtt.CodeWorks.Tokenizer;
using Gtt.CodeWorks.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Gtt.CodeWorks
{
    public class CoreDependencies
    {
        public CoreDependencies(
            ILoggerFactory loggerFactory,
            IServiceLogger serviceLogger,
            ICodeWorksTokenizer tokenizer,
            IRateLimiter rateLimiter,
            IDistributedLockService distributedLockService,
            IServiceEnvironmentResolver environmentResolver,
            IRequestValidator requestValidator,
            IChainedServiceResolver chainedServiceResolver, 
            IUserResolver userResolver)
        {
            LoggerFactory = loggerFactory;
            ServiceLogger = serviceLogger;
            Tokenizer = tokenizer;
            RateLimiter = rateLimiter;
            DistributedLockService = distributedLockService;
            EnvironmentResolver = environmentResolver;
            RequestValidator = requestValidator;
            ChainedServiceResolver = chainedServiceResolver;
            UserResolver = userResolver;
        }

        private CoreDependencies()
        {
            LoggerFactory = new NullLoggerFactory();
            ServiceLogger = NullServiceLogger.Instance;
            Tokenizer = new CodeWorksTokenizer(new NullTokenService());
            RateLimiter = new InMemoryRateLimiter();
            DistributedLockService = new InMemoryDistributedLock();
            EnvironmentResolver = new NonProductionEnvironmentResolver();
            RequestValidator = NullRequestValidator.Instance;
            ChainedServiceResolver = new DefaultChainedServiceResolver(LoggerFactory.CreateLogger<DefaultChainedServiceResolver>());
            UserResolver = NullUserResolver.Instance;
        }

        public IServiceLogger ServiceLogger { get; }
        public ICodeWorksTokenizer Tokenizer { get; }
        public ILoggerFactory LoggerFactory { get; }
        public IRateLimiter RateLimiter { get; }
        public IDistributedLockService DistributedLockService { get; }
        public IServiceEnvironmentResolver EnvironmentResolver { get; }
        public IRequestValidator RequestValidator { get; }
        public IChainedServiceResolver ChainedServiceResolver { get; }
        public IUserResolver UserResolver { get; }
        public static CoreDependencies NullDependencies => new CoreDependencies();

    }

    public class NullUserResolver : IUserResolver
    {
        private NullUserResolver()
        {
            
        }
        public Task<UserResolverResult> GetUserOrDefault(string authToken, Guid correlationId, CancellationToken cancellationToken)
        {
            return Task.FromResult(new UserResolverResult(UserAuthStatus.NoUser));
        }

        public static NullUserResolver Instance => new NullUserResolver();
    }
}
