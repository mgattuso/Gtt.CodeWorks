using System;
using System.Collections.Generic;
using System.Linq;

namespace Gtt.CodeWorks.Duplicator
{
    public class TypeMetaData
    {
        public TypeMetaData(Type t)
        {
            Type = t;
            FundamentalType = this;
        }

        public string Name => Type.Name;
        public string ClassNormalizedName
        {
            get
            {
                var name = Name;
                var nname = name.Split('`')[0];
                if (!HasGenerics) return nname;
                return $"{nname}<{ string.Join(",", OrderedGenericArguments.Select(x => x.Type.FundamentalType.PropertyNormalizedName).ToArray())}>";
            }
        }

        public string ClassInheritanceNormalizedName
        {
            get
            {
                var name = Name;
                var nname = name.Split('`')[0];
                if (!HasGenerics) return nname;
                return $"{nname}<{ string.Join(",", OrderedGenericInstanceArguments.Select(x => x.Type.PropertyNormalizedName).ToArray())}>";
            }
        }

        public string PropertyNormalizedName
        {
            get
            {
                var name = Name;
                var nname = name.Split('`')[0];
                if (!HasGenerics) return nname;
                return $"{nname}<{ string.Join(",", OrderedGenericArguments.Select(x => x.TName).ToArray())}>";
            }
        }
        public Type Type { get; }
        public TypeFormat Format { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsNullable { get; set; }
        public bool IsCollection { get; set; }
        public bool IsTemplateType { get; set; }
        public bool HasGenerics { get; set; }
        public string Namespace { get; set; }
        public List<GenericTypeMetaData> OrderedGenericArguments { get; set; } = new List<GenericTypeMetaData>();
        public List<GenericTypeMetaData> OrderedGenericInstanceArguments { get; set; } = new List<GenericTypeMetaData>();
        public TypeMetaData HierarchyParent { get; set; }
        public TypeMetaData DeclaredParent { get; set; }
        public TypeMetaData FundamentalType { get; set; }
        public Dictionary<int, string> EnumValues { get; set; } = new Dictionary<int, string>();
        public List<PropertyMetaData> Properties { get; set; } = new List<PropertyMetaData>();
        public bool IsWritable
        {
            get
            {
                if (IsTemplateType) return false;
                if (IsCollection) return false;
                if (!OrderedGenericArguments.Any()) return true;
                bool result = true;
                for (int i = 0; i < OrderedGenericArguments.Count; i++)
                {
                    var og = OrderedGenericArguments[i];
                    var od = OrderedGenericInstanceArguments[i];
                    if (og.Type != od.Type)
                    {
                        result = false;
                    }
                }
                return result;
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as TypeMetaData;
            if (other == null) return false;
            return Equals(other.Type, this.Type);
        }

        public override int GetHashCode()
        {
            return this.Type.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}