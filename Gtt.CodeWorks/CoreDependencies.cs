using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Gtt.CodeWorks
{
    public class CoreDependencies
    {
        public CoreDependencies(
            ILogger logger, 
            IServiceLogger serviceLogger, 
            ICodeWorksTokenizer tokenizer, 
            IRateLimiter rateLimiter,
            IDistributedLockService distributedLockService,
            IServiceEnvironmentResolver environmentResolver)
        {
            Logger = logger;
            ServiceLogger = serviceLogger;
            Tokenizer = tokenizer;
            RateLimiter = rateLimiter;
            DistributedLockService = distributedLockService;
            EnvironmentResolver = environmentResolver;
        }

        public CoreDependencies()
        {
            Logger = NullLogger.Instance;
            ServiceLogger = NullServiceLogger.Instance;
            Tokenizer = NullTokenizer.SkipTokenization;
            RateLimiter = new InMemoryRateLimiter();
            DistributedLockService = new InMemoryDistributedLock();
            EnvironmentResolver = new NonProductionEnvironmentResolver();
        }

        public IServiceLogger ServiceLogger { get; }
        public ICodeWorksTokenizer Tokenizer { get; }
        public ILogger Logger { get; }
        public IRateLimiter RateLimiter { get; }
        public IDistributedLockService DistributedLockService { get; }
        public IServiceEnvironmentResolver EnvironmentResolver { get; }
    }
}
