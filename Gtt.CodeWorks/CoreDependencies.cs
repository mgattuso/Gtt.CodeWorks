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
            CodeWorksEnvironment environment)
        {
            Logger = logger;
            ServiceLogger = serviceLogger;
            Tokenizer = tokenizer;
            Environment = environment;
            RateLimiter = rateLimiter;
            DistributedLockService = distributedLockService;
        }

        public CoreDependencies()
        {
            Environment = CodeWorksEnvironment.NonProduction;
            Logger = NullLogger.Instance;
            ServiceLogger = NullServiceLogger.Instance;
            Tokenizer = NullTokenizer.SkipTokenization;
            RateLimiter = new InMemoryRateLimiter();
            DistributedLockService = new InMemoryDistributedLock();
        }

        public IServiceLogger ServiceLogger { get; }
        public ICodeWorksTokenizer Tokenizer { get; }
        public ILogger Logger { get; }
        public CodeWorksEnvironment Environment { get; }
        public IRateLimiter RateLimiter { get; }
        public IDistributedLockService DistributedLockService { get; }
    }
}
