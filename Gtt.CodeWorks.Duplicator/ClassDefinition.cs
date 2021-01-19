using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Gtt.CodeWorks.Duplicator
{
    public class ClassDefinition
    {
        public ClassDefinition(Type t, ModelTree tree, ClassDefinition parent = null)
        {
            Type = t;
            Name = t.Name;
            Tree = tree;

            parent?.NestClasses.Add(this);

            var props = t.GetProperties(BindingFlags.Public
                                        | BindingFlags.Instance
                                        | BindingFlags.DeclaredOnly);

            var parentType = t.BaseType;
            if (parentType != null && parentType != typeof(object))
            {
                BaseClass = $": {parentType.Name}";
                tree.AddType(parentType);
                AddUsing(parentType.Namespace);
            }

            if (t.IsAbstract)
            {
                Prefix = "abstract";
            }

            foreach (var p in props)
            {
                if (p.CanRead && p.CanWrite)
                {
                    new PropertyDefinition(this, p);
                }
            }
        }

        public Type Type { get; }
        public string Name { get; }

        public string BaseClass { get; }

        public string Prefix { get; }

        public ModelTree Tree { get; }
        public List<PropertyDefinition> Properties { get; } = new List<PropertyDefinition>();
        public List<ClassDefinition> NestClasses { get; } = new List<ClassDefinition>();
        public List<EnumDefinition> Enums { get; } = new List<EnumDefinition>();

        public void AddUsing(string u)
        {
            Tree.AddUsings(u);
        }

        internal void AddNestedClass(Type t)
        {
            if (NestClasses.Any(nc => nc.Type == t))
            {
                return;
            }
            new ClassDefinition(t, Tree, this);
        }

        internal void AddType(Type ct)
        {
            if (ct.DeclaringType == null)
            {
                Tree.AddType(ct);
                return;
            }

            if (ct.DeclaringType == this.Type)
            {
                AddNestedClass(ct);
                return;
            }

            var an = NestClasses.FirstOrDefault(nc => nc.Type == ct.DeclaringType);
            if (an != null)
            {
                an.AddNestedClass(ct);
                return;
            }

            ClassDefinition cd = Tree.FindClassDefinitionForType(ct);
            cd?.AddNestedClass(ct);
        }

        internal void AddEnum(EnumDefinition enumDefinition)
        {
            if (Enums.Any(e => e.Type == enumDefinition.Type)) return;

            enumDefinition.Parent = this;
            Enums.Add(enumDefinition);
        }

        public string Write()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"public {Prefix} class {Name} {BaseClass} {{");

            foreach (var p in Properties)
            {
                sb.AppendLine($"\t{p.Write()}");
            }

            foreach (var c in NestClasses)
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