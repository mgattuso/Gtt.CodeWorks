using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gtt.CodeWorks.Duplicator
{
    public class ModelTree
    {
        private readonly List<NamespaceDefinition> _namespaces = new List<NamespaceDefinition>();

        private NamespaceDefinition GetNamespace(string name)
        {
            var ns = _namespaces.FirstOrDefault(n => n.Name == name);
            if (ns == null)
            {
                ns = new NamespaceDefinition(name, this);
                _namespaces.Add(ns);
            }
            return ns;
        }

        public IEnumerable<NamespaceDefinition> Namespaces => _namespaces;

        public void AddType(Type t)
        {
            var tns = t.Namespace;
            var ns = GetNamespace(tns);
            if (t.IsClass)
            {
                ns.AddClass(t);
            }
            else if (t.IsEnum)
            {
                ns.AddEnum(t);
            }
        }

        internal ClassDefinition FindClassDefinitionForType(Type ct)
        {
            throw new NotImplementedException("FindClassDefinitionForType");
        }

        private HashSet<string> Usings { get; set; } = new HashSet<string>();

        public string Write()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var u in Usings.OrderBy(x => x))
            {
                sb.AppendLine($"using {u};");
            }

            foreach (var n in Namespaces)
            {
                sb.AppendLine();
                sb.Append(n.Write());
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public void AddUsings(string u)
        {
            if (Namespaces.Select(x => x.Name).Contains(u))
            {
                return;
            }
            Usings.Add(u);
        }
    }
}