using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Gtt.CodeWorks.Duplicator
{
    public class Copier
    {
        private readonly List<Type> _types = new List<Type>();

        private List<Assembly> _onlyGenerateForAssemblies = new List<Assembly>();
        private readonly List<string> _ignoreNamespacePrefixes = new List<string>();

        /// <summary>
        /// Ignore the original modifiers and always use getters and setters.
        /// </summary>
        public bool AlwaysGetAndSet { get; set; } = true;
        public string ForceNamespace { get; set; }
        public (string replace, string with)? ReplaceNamespace { get; set; }

        public Copier()
        {
            ClearIgnoreNamespacePrefixes();
        }

        public void AddType(Type t)
        {
            _types.Add(t);
        }

        public void ClearIgnoreNamespacePrefixes(bool clearBuiltInPrefixes = false)
        {
            _ignoreNamespacePrefixes.Clear();
            if (!clearBuiltInPrefixes)
            {
                _ignoreNamespacePrefixes.Add("Microsoft");
                _ignoreNamespacePrefixes.Add("System");
            }
        }

        public void AddIgnoreNamespacePrefix(string namespacePrefix)
        {
            _ignoreNamespacePrefixes.Add(namespacePrefix);
        }

        public void ClearOnlyGenerateForAssemblies()
        {
            _onlyGenerateForAssemblies.Clear();
        }

        public void LimitOutputToAssembly(Assembly assembly)
        {
            _onlyGenerateForAssemblies.Add(assembly);
        }

        public void LimitOutputToAssemblyOfType(Type t)
        {
            _onlyGenerateForAssemblies.Add(t.Assembly);
        }

        public string Process()
        {
            var l = new List<TypeMetaData>();

            foreach (var t in _types)
            {
                GetTypesUsedInType(t, l);
            }

            foreach (var t in l)
            {
                foreach (var p in t.Type.GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                {
                    var gp = p.PropertyType;

                    var et = l.FirstOrDefault(x => x.Type == gp);
                    var pr = new PropertyMetaData
                    {
                        Name = p.Name,
                        Parent = t,
                        Property = et,
                        Getter = p.CanRead,
                        Setter = p.CanWrite
                    };
                    t.Properties.Add(pr);
                }
            }

            StringBuilder sb = new StringBuilder();

            var filteredList = l;

            foreach (var prefix in _ignoreNamespacePrefixes)
            {
                filteredList = filteredList.Where(x => x.Namespace == null || !x.Namespace.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            var assemblyList = new List<TypeMetaData>();

            foreach (var assembly in _onlyGenerateForAssemblies)
            {
                var r = filteredList.Where(x => x.Type.Assembly == assembly);
                assemblyList.AddRange(r);
            }

            if (_onlyGenerateForAssemblies.Any())
            {
                filteredList = assemblyList;
            }


            var usings = l.Where(x => !string.IsNullOrWhiteSpace(x.Namespace)).Select(x => x.Namespace).Distinct();
            foreach (var u in usings)
            {
                sb.AppendLine($"using {u};");
            }

            sb.AppendLine();

            foreach (var n in filteredList.GroupBy(x => x.Namespace))
            {
                if (!string.IsNullOrWhiteSpace(n.Key))
                {
                    sb.AppendLine($"namespace {n.Key} {{");
                }

                // WRITE CLASSES
                foreach (var t in n.Where(x => x.DeclaredParent == null && x.IsWritable).Select(x => x.FundamentalType).Distinct())
                {
                    sb.Append(Typewriter(t, l));
                }

                if (!string.IsNullOrWhiteSpace(n.Key))
                {
                    sb.AppendLine($"}}");
                }
            }

            return sb.ToString();
        }

        public void GetTypesUsedInType(Type originalType, List<TypeMetaData> types)
        {
            if (originalType == null) return;

            var t = new TypeMetaData(originalType);
            if (!types.Contains(t))
            {
                if (!string.IsNullOrWhiteSpace(ForceNamespace))
                {
                    t.Namespace = ForceNamespace;
                }
                else
                {
                    t.Namespace = t.Type.Namespace;
                }

                if (ReplaceNamespace != null)
                {
                    var rn = ReplaceNamespace.Value;
                    if (t.Namespace != null)
                        t.Namespace = t.Namespace.Replace(rn.replace, rn.with);
                }

                types.Add(t);

                // ADD DECLARED PARENT
                var d = t.Type.DeclaringType;
                if (d != null)
                {
                    GetTypesUsedInType(d, types);
                    t.DeclaredParent = types.First(ty => ty.Type == d);
                }

                // ADD HIERARCHY PARENT
                var b = t.Type.BaseType;
                if (b != null && b != typeof(object) && b != typeof(ValueType) && b != typeof(Enum))
                {
                    GetTypesUsedInType(b, types);
                    t.HierarchyParent = types.First(ty => ty.Type == t.Type.BaseType);
                }

                // GET FUNDAMENTAL TYPE (FROM LIST OF X)
                var ut = Nullable.GetUnderlyingType(t.Type);
                if (ut != null)
                {
                    t.IsNullable = true;
                    GetTypesUsedInType(ut, types);
                    var ft = types.First(x => x.Type == ut);
                    t.FundamentalType = ft;
                }

                // LOOK FOR ELEMENT TYPE (EG ARRAY)
                if (t.Type.HasElementType)
                {
                    if (t.Type.IsArray)
                    {
                        t.IsCollection = true;
                    }
                    var at = t.Type.GetElementType();
                    if (at != null)
                    {
                        GetTypesUsedInType(at, types);
                        var ft = types.First(x => x.Type == at);
                        t.FundamentalType = ft;
                    }
                }

                // LOOK FOR COLLECTIONS
                if (typeof(ICollection).IsAssignableFrom(t.Type))
                {
                    t.IsCollection = true;
                    var generics = t.Type.GetGenericArguments();
                    if (generics.Length == 1)
                    {
                        var gt = t.Type.GetGenericArguments()[0];
                        GetTypesUsedInType(gt, types);
                        var ft = types.First(x => x.Type == gt);
                        t.FundamentalType = ft;
                    }
                }


                // HANDLE GENERICS

                if (!t.Type.IsGenericParameter && (t.Type.IsClass || t.Type.IsEnum || t.Type.IsValueType))
                {
                    if (t.Type.IsEnum)
                    {
                        t.Format = TypeFormat.Enum;
                        var names = Enum.GetNames(t.Type);
                        for (int i = 0; i < names.Length; i++)
                        {
                            var val = (int)Enum.Parse(t.Type, names[i]);
                            t.EnumValues.Add(val, names[i]);
                        }
                    }
                    else if (t.Type.IsValueType)
                    {
                        t.Format = TypeFormat.Struct;
                    }
                    else
                    {
                        t.Format = TypeFormat.Class;
                    }

                    if (t.Type.IsAbstract)
                    {
                        t.IsAbstract = true;
                    }

                    if (t.Type.IsGenericType)
                    {
                        t.HasGenerics = true;
                        Type[] g = t.Type.GetGenericArguments();
                        foreach (Type a in g)
                        {
                            GetTypesUsedInType(a, types);
                        }

                        var gtd = t.Type.GetGenericTypeDefinition();
                        GetTypesUsedInType(gtd, types);
                        var gttt = types.First(ty => ty.Type == gtd);
                        if (gttt != t)
                        {
                            t.HierarchyParent = gttt;
                        }
                        Type[] ga = gtd.GetGenericArguments();
                        Type[] nga = t.Type.GetGenericArguments();
                        foreach (Type a in ga)
                        {
                            // UNCOMMENT THIS TO ADD GENERIC TYPES (E.g. T TO THE LIST)
                            GetTypesUsedInType(a, types);
                            var gta = types.First(ty => ty.Type == a);
                            GenericTypeMetaData gmd = new GenericTypeMetaData(gta);
                            gmd.TName = a.Name;
                            gmd.Type.IsTemplateType = true;
                            t.OrderedGenericArguments.Add(gmd);

                            var gc = a.GetGenericParameterConstraints();
                            foreach (var gt in gc)
                            {
                                GetTypesUsedInType(gt, types);
                                var gtct = types.First(x => x.Type == gt);
                                if (gtct.Type != typeof(ValueType))
                                {
                                    gmd.TypeConstraints.Add(gtct);
                                }
                            }
                            var attrs = a.GenericParameterAttributes;
                            if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                            {
                                gmd.GenericConstraints.Add("struct");
                            }

                            if (attrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) && !attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                            {
                                gmd.GenericConstraints.Add("new()");
                            }

                            if (attrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
                            {
                                gmd.GenericConstraints.Add("class");
                            }
                        }

                        foreach (Type a in nga)
                        {
                            GetTypesUsedInType(a, types);
                            var gta = types.First(ty => ty.Type == a);
                            GenericTypeMetaData gmd = new GenericTypeMetaData(gta)
                            {
                                TName = a.Name
                            };
                            t.OrderedGenericInstanceArguments.Add(gmd);
                        }
                    }

                }

                foreach (var p in t.Type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
                {
                    GetTypesUsedInType(p.PropertyType, types);
                }


            }
        }

        private string Typewriter(TypeMetaData t, List<TypeMetaData> types)
        {
            StringBuilder sb = new StringBuilder();

            string name = t.FundamentalType.ClassNormalizedName;

            if (t.HierarchyParent != null)
            {
                name = $"{name} : {t.HierarchyParent.ClassInheritanceNormalizedName}";
            }

            switch (t.Format)
            {
                case TypeFormat.Class:
                    sb.AppendLine($"public {(t.IsAbstract ? "abstract" : "")} class {name} {{");

                    foreach (var p in t.Properties)
                    {
                        string modifiers = $"{{ {(p.Getter || AlwaysGetAndSet ? "get;" : "")} {(p.Setter || AlwaysGetAndSet ? "set;" : "")} }}";
                        sb.AppendLine($"public {p.Property.ClassInheritanceNormalizedName} {p.Name} {modifiers}");
                    }

                    foreach (var st in types.Where(ty => ty.DeclaredParent == t && ty.IsWritable))
                    {
                        sb.Append(Typewriter(st, types));
                    }

                    break;
                case TypeFormat.Enum:
                    sb.AppendLine($"public enum {name} {{");
                    foreach (var kv in t.EnumValues.OrderBy(x => x.Key))
                    {
                        sb.AppendLine($"{kv.Value} = {kv.Key},");
                    }
                    break;
                case TypeFormat.Struct:
                    sb.AppendLine($"public struct {name} {{");
                    break;
            }

            sb.AppendLine("}");

            return sb.ToString();

        }

    }

    public class GenericTypeMetaData
    {
        public GenericTypeMetaData(TypeMetaData t)
        {
            Type = t;
        }

        public string TName { get; set; }
        public List<TypeMetaData> TypeConstraints { get; set; } = new List<TypeMetaData>();
        public List<string> GenericConstraints { get; set; } = new List<string>();
        public TypeMetaData Type { get; }

        public override bool Equals(object obj)
        {
            var other = obj as GenericTypeMetaData;
            if (other == null) return false;
            return Equals(other.Type, this.Type);
        }

        public override int GetHashCode()
        {
            return this.Type.GetHashCode();
        }
    }

    public class PropertyMetaData
    {
        public PropertyMetaData()
        {

        }

        public string Name { get; set; }
        public TypeMetaData Parent { get; set; }
        public TypeMetaData Property { get; set; }
        public bool Getter { get; set; }
        public bool Setter { get; set; }
    }

    public enum TypeFormat
    {
        @Class,
        @Struct,
        @Enum
    }

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
                var t = this;
                string upperLevel = "";
                if (t.DeclaredParent != null && !t.IsTemplateType)
                {
                    upperLevel = t.DeclaredParent != null ? t.DeclaredParent.ClassInheritanceNormalizedName + "." : "";
                }
                
                var name = Name;
                var nname = name.Split('`')[0];
                if (!HasGenerics) return upperLevel + nname;
                return $"{upperLevel}{nname}<{ string.Join(",", OrderedGenericInstanceArguments.Select(x => x.Type.PropertyNormalizedName).ToArray())}>";
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
