using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gtt.CodeWorks.Duplicator
{
    public class NamespaceDefinition
    {
        public NamespaceDefinition(string name, ModelTree tree)
        {
            Name = name;
            Tree = tree;
        }

        public string Name { get; }
        public ModelTree Tree { get; }
        public List<ClassDefinition> Classes { get; set; } = new List<ClassDefinition>();
        public List<EnumDefinition> Enums { get; set; } = new List<EnumDefinition>();

        public void AddClass(Type t)
        {
            if (!t.IsClass) throw new ArgumentException($"{t} is not a class");
            var existing = Classes.Any(c => c.Type == t);
            if (existing) return;

            var cd = new ClassDefinition(t, Tree);
            Classes.Add(cd);
        }

        internal void AddEnum(Type t)
        {
            var en = new EnumDefinition(t, null);
            Enums.Add(en);
        }

        internal string Write()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"namespace {Name} {{");

            foreach (var c in Classes)
            {
                sb.AppendLine();
                sb.Append(c.Write());
                sb.AppendLine();
            }

            foreach (var e in Enums)
            {
                sb.AppendLine();
                sb.Append(e.Write());
                sb.AppendLine();
            }

            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}