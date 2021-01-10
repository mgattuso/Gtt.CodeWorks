using System.Collections.Generic;
using System.Text;
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
            IChainedServiceResolver chainedServiceResolver)
        {
            LoggerFactory = loggerFactory;
            ServiceLogger = serviceLogger;
            Tokenizer = tokenizer;
            RateLimiter = rateLimiter;
            DistributedLockService = distributedLockService;
            EnvironmentResolver = environmentResolver;
            RequestValidator = requestValidator;
            ChainedServiceResolver = chainedServiceResolver;
        }

        private CoreDependencies()
        {
            LoggerFactory = new NullLoggerFactory();
            ServiceLogger = NullServiceLogger.Instance;
            Tokenizer = NullTokenizer.SkipTokenization;
            RateLimiter = new InMemoryRateLimiter();
            DistributedLockService = new InMemoryDistributedLock();
            EnvironmentResolver = new NonProductionEnvironmentResolver();
            RequestValidator = NullRequestValidator.Instance;
            ChainedServiceResolver = new DefaultChainedServiceResolver();
        }

        public IServiceLogger ServiceLogger { get; }
        public ICodeWorksTokenizer Tokenizer { get; }
        public ILoggerFactory LoggerFactory { get; }
        public IRateLimiter RateLimiter { get; }
        public IDistributedLockService DistributedLockService { get; }
        public IServiceEnvironmentResolver EnvironmentResolver { get; }
        public IRequestValidator RequestValidator { get; }
        public IChainedServiceResolver ChainedServiceResolver { get; }
        public static CoreDependencies NullDependencies => new CoreDependencies();

    }
}
