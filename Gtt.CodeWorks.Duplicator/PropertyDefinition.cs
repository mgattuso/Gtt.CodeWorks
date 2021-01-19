using System;
using System.Collections;
using System.Reflection;

namespace Gtt.CodeWorks.Duplicator
{
    public class PropertyDefinition
    {
        public PropertyDefinition(ClassDefinition parent, PropertyInfo p)
        {
            Class = parent;
            parent.Properties.Add(this);
            Name = p.Name;
            var t = p.PropertyType;
            PropertyType = t;
            FundamentalType = t;
            Class.AddUsing(t.Namespace);
            var propParent = t.DeclaringType;
            bool nested = false;

            if (propParent != null && propParent != parent.Type)
            {
                parent.Tree.AddType(propParent);
            }

            var nt = Nullable.GetUnderlyingType(PropertyType);
            if (nt != null)
            {
                Suffix = "?";
                FundamentalType = nt;
            }

            var ct = GetCollectionType();
            if (ct != null)
            {
                FundamentalType = ct;

                if (t.IsArray)
                {
                    Suffix = "[]";
                }
                else
                {
                    Prefix = t.Name.Replace("`1", "<");
                    Suffix = ">";
                }

                if (ct.DeclaringType != null)
                {
                    nested = true;
                    if (ct.DeclaringType == parent.Type)
                    {
                        parent.AddNestedClass(ct);
                    }
                }
                else
                {
                    parent.AddType(ct);
                }
            }

            if (FundamentalType.IsEnum)
            {
                new EnumDefinition(FundamentalType, this.Class);
            }

            if (FundamentalType.IsClass && !nested)
            {
                parent.Tree.AddType(FundamentalType);
            }


        }

        private ClassDefinition Class { get; }
        private Type PropertyType { get; }
        private Type FundamentalType { get; }
        private string Prefix { get; set; }
        private string Suffix { get; set; }

        public string Type => $"{Prefix}{FundamentalType.Name}{Suffix}";

        public string Name { get; }

        public Type GetEnumType()
        {
            if (PropertyType.IsEnum)
            {
                return PropertyType;
            }

            return null;
        }

        public Type GetCollectionType()
        {
            if (!typeof(IEnumerable).IsAssignableFrom(PropertyType)) return null;

            if (PropertyType.IsArray)
            {
                return PropertyType.GetElementType();
            }

            else if (PropertyType.IsGenericType)
            {
                var generics = PropertyType.GetGenericArguments();
                if (generics.Length == 1)
                {
                    return PropertyType.GetGenericArguments()[0];
                }
            }
            else
            {
                return null;
            }

            return null;
        }

        public string Write()
        {
            return $"public {Type} {Name} {{ get; set; }}";
        }
    }
}