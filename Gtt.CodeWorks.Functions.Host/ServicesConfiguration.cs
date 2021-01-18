using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gtt.CodeWorks.Clients.HttpRequest;
using Gtt.CodeWorks.DataAnnotations;
using Gtt.CodeWorks.Serializers.TextJson;
using Gtt.CodeWorks.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Gtt.CodeWorks.Functions.Host
{
    public static class ServicesConfiguration
    {

        public static IServiceCollection ConfigureCodeWorksAll<TServiceFromAssembly>(this IServiceCollection services, string namespacePrefixToIgnore = "")
            where TServiceFromAssembly : IServiceInstance
        {
            services.ConfigureLogging()
                    .ConfigureCoreDependencies()
                    .ConfigureHttp()
                    .ConfigureServices<TServiceFromAssembly>(namespacePrefixToIgnore);
            return services;
        }

        public static IServiceCollection ConfigureLogging(this IServiceCollection services)
        {
            services.AddLogging();
            services.AddTransient<ILogObjectSerializer, JsonLogObjectSerializer>();
            services.AddTransient<IServiceLogger, ServiceLogger>();
            return services;
        }

        public static IServiceCollection ConfigureCoreDependencies(this IServiceCollection services)
        {
            services.AddTransient<ICodeWorksTokenizer>(cfg => NullTokenizer.SkipTokenization);
            services.AddTransient<IRateLimiter>(cfg => new InMemoryRateLimiter());
            services.AddTransient<IDistributedLockService>(cfg => new InMemoryDistributedLock());
            services.AddTransient<IServiceEnvironmentResolver>(cfg => new NonProductionEnvironmentResolver());
            services.AddTransient<IRequestValidator, DataAnnotationsRequestValidator>();
            services.AddTransient<CoreDependencies>();
            return services;
        }

        public static IServiceCollection ConfigureHttp(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddTransient<IHttpDataSerializer, HttpJsonDataSerializer>();
            services.AddTransient<HttpRequestMessageConverter>();
            services.AddTransient<HttpRequestMessageRunner>();
            return services;
        }

        public static IServiceCollection ConfigureServices<TServiceFromAssembly>(this IServiceCollection services, string namespacePrefixToIgnore = "") where TServiceFromAssembly : IServiceInstance
        {
            foreach (var svc in GetConcreteInstancesOf<IServiceInstance>(typeof(TServiceFromAssembly)))
            {
                Console.WriteLine($"Registering Service {svc.Name}");
                services.AddScoped(svc);
                services.AddScoped(cfg => (IServiceInstance)cfg.GetService(svc));
                services.AddScoped<IChainedServiceResolver, DefaultChainedServiceResolver>();
            }
            services.AddScoped<IServiceResolver>(cfg => new ServiceResolver(cfg.GetServices<IServiceInstance>(), new ServiceResolverOptions
            {
                NamespacePrefixToIgnore = namespacePrefixToIgnore
            }));
            return services;
        }

        private static IEnumerable<Type> GetConcreteInstancesOf<T>(Type sampleTypeFromAssembly)
        {
            var a = sampleTypeFromAssembly.Assembly;
            var cls = a.GetTypes().Where(p => typeof(T).IsAssignableFrom(p) && !p.IsAbstract);
            return cls;
        }
    }
}
