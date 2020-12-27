using System;
using System.Collections.Generic;
using System.Text;
using Gtt.CodeWorks.Clients;

namespace Gtt.CodeWorks
{
    public interface IDependencyTracker
    {
        void AddDependency(IClientConverter instance);
        IEnumerable<IClientConverter> GetAllDependencies();
    }

    public class DependencyTracker : IDependencyTracker
    {
        private readonly HashSet<IClientConverter> _dependencies = new HashSet<IClientConverter>();
        public void AddDependency(IClientConverter instance)
        {
            _dependencies.Add(instance);
        }

        public IEnumerable<IClientConverter> GetAllDependencies()
        {
            return _dependencies;
        }
    }
}
