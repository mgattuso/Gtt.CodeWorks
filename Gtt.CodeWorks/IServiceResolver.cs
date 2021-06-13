using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gtt.CodeWorks
{
    public interface IServiceResolver
    {
        IServiceInstance GetInstanceByName(string name);
        IServiceInstance[] GetRegistered();
        string GetRegisteredNameFromFullName(string fullName);
        string[] GetPathSegments(string fullName);
    }

    public class BetterServiceResolver : IServiceResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceResolverOptions _options;
        private IDictionary<string, Type> _types = new Dictionary<string, Type>();

        public BetterServiceResolver(IList<Type> serviceTypes, IServiceProvider serviceProvider, ServiceResolverOptions options = default)
        {
            _options = options ?? new ServiceResolverOptions();
            _serviceProvider = serviceProvider;

            foreach (var serviceType in serviceTypes)
            {
                var ri = _serviceProvider.GetService(serviceType);
                if (!(ri is IServiceInstance instance)) continue;

                string name = instance.Name;

                if (_options.IncludeNamespaceInRegistration)
                    name = GetRegisteredNameFromFullName(instance.FullName);

                if (_options.CaseSensitive)
                    name = name.ToUpperInvariant();

                var existingEntry = _types.TryGetValue(name, out var foundType);
                if (existingEntry)
                {
                    throw new Exception($"Existing entry for key {{name}} already registered with {instance.FullName}");
                }
                _types[name] = serviceType;
            }
        }

        public IServiceInstance GetInstanceByName(string name)
        {
            if (_options.CaseSensitive)
                name = name.ToUpperInvariant();

            _types.TryGetValue(name, out var serviceInstance);
            var instance = _serviceProvider.GetService(serviceInstance);
            return instance as IServiceInstance;
        }

        public IServiceInstance[] GetRegistered()
        {
            return _types.OrderBy(x => x.Key).Select(x =>
            {
                var instance = _serviceProvider.GetService(x.Value);
                return instance as IServiceInstance;
            }).ToArray();
        }

        public string GetRegisteredNameFromFullName(string fullName)
        {
            string separator = _options.NamespaceSeparator ?? "";
            string[] segments = GetRawSegments(fullName);
            return string.Join(separator, segments);
        }

        public string[] GetPathSegments(string fullName)
        {
            var regName = GetRawSegments(fullName);
            if (regName.Length == 1)
            {
                return new string[0];
            }

            return regName.Take(regName.Length - 1).ToArray();
        }

        public string[] GetRawSegments(string fullName)
        {
            if (!_options.IncludeNamespaceInRegistration)
                throw new Exception("Cannot call this method if the options.IncludeNamespaceInRegistration is true");

            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(fullName));

            if (_options.NamespaceDepth.HasValue)
            {
                string[] fn = fullName.Split('.');
                var depth = _options.NamespaceDepth.Value + 1;
                if (fn.Length > depth)
                {
                    var keep = fn.Skip(fn.Length - depth);
                    return keep.ToArray();
                }
            }

            if (!string.IsNullOrWhiteSpace(_options.NamespacePrefixToIgnore))
            {
                var s = fullName.Replace(_options.NamespacePrefixToIgnore, "");
                if (s.StartsWith("."))
                {
                    s = s.Substring(1, s.Length - 1);
                }

                return s.Split('.');
            }

            return fullName.Split('.');
        }
    }

    public class ServiceResolver : IServiceResolver
    {
        private readonly ServiceResolverOptions _options;
        private readonly Dictionary<string, IServiceInstance> _instances = new Dictionary<string, IServiceInstance>();

        public ServiceResolver(IEnumerable<IServiceInstance> instances, ServiceResolverOptions options = default)
        {
            _options = options ?? new ServiceResolverOptions();
            instances = instances?.ToArray() ?? new IServiceInstance[0];
            foreach (var instance in instances)
            {
                string name = instance.Name;

                if (_options.IncludeNamespaceInRegistration)
                    name = GetRegisteredNameFromFullName(instance.FullName);

                if (_options.CaseSensitive)
                    name = name.ToUpperInvariant();

                var existingEntry = _instances.TryGetValue(name, out var foundInstance);
                if (existingEntry)
                {
                    throw new Exception($"Existing entry for key {{name}} already registered with {foundInstance.FullName}");
                }
                _instances[name] = instance;
            }
        }

        public IServiceInstance GetInstanceByName(string name)
        {
            if (_options.CaseSensitive)
                name = name.ToUpperInvariant();

            _instances.TryGetValue(name, out var serviceInstance);
            return serviceInstance;
        }

        public IServiceInstance[] GetRegistered()
        {
            return _instances.OrderBy(x => x.Key).Select(x => x.Value).ToArray();
        }

        public string GetRegisteredNameFromFullName(string fullName)
        {
            string separator = _options.NamespaceSeparator ?? "";
            string[] segments = GetRawSegments(fullName);
            return string.Join(separator, segments);
        }

        public string[] GetPathSegments(string fullName)
        {
            var regName = GetRawSegments(fullName);
            if (regName.Length == 1)
            {
                return new string[0];
            }

            return regName.Take(regName.Length - 1).ToArray();
        }

        private string[] GetRawSegments(string fullName)
        {
            if (!_options.IncludeNamespaceInRegistration)
                throw new Exception("Cannot call this method if the options.IncludeNamespaceInRegistration is true");

            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(fullName));

            if (_options.NamespaceDepth.HasValue)
            {
                string[] fn = fullName.Split('.');
                var depth = _options.NamespaceDepth.Value + 1;
                if (fn.Length > depth)
                {
                    var keep = fn.Skip(fn.Length - depth);
                    return keep.ToArray();
                }
            }

            if (!string.IsNullOrWhiteSpace(_options.NamespacePrefixToIgnore))
            {
                var s = fullName.Replace(_options.NamespacePrefixToIgnore, "");
                if (s.StartsWith("."))
                {
                    s = s.Substring(1, s.Length - 1);
                }

                return s.Split('.');
            }

            return fullName.Split('.');
        }
    }

    public class ServiceResolverOptions
    {
        /// <summary>
        /// Should the namespace be incorporated into the registration. Defaults to <value>true</value>
        /// </summary>
        public bool IncludeNamespaceInRegistration { get; set; } = true;

        /// <summary>
        /// If the IncludeNamespaceInRegistration is set to true then set the part of the namespace to be ignored
        /// </summary>
        public string NamespacePrefixToIgnore { get; set; }
        /// <summary>
        /// Set the separator for the service registration. Defaults to <value>/</value> to support url routing with namespace
        /// </summary>
        public string NamespaceSeparator { get; set; } = "/";

        /// <summary>
        /// Sets the max number of namespaces to include from the right hand side. Should not be used
        /// with <code>NamespacePrefixToIgnore</code>.
        /// </summary>
        public int? NamespaceDepth { get; set; }

        /// <summary>
        /// Is the service registration lookup case sensitive. Default value is <value>false</value>
        /// </summary>
        public bool CaseSensitive { get; set; } = false;
    }
}
