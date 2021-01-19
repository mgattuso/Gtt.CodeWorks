using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gtt.CodeWorks.Duplicator
{
    public class EnumDefinition
	{
		public EnumDefinition(Type en, ClassDefinition cd = null)
		{
			Type = en;
			Name = en.Name;
			var names = Enum.GetNames(en);
			foreach (var t in names)
            {
                var val = (int)Enum.Parse(en, t);
                Values.Add(val, t);
            }

			if (cd == null) return;

			if (en.DeclaringType != null)
			{
				if (en.DeclaringType == cd.Type)
				{
					cd.AddEnum(this);
				}
				else if (cd.NestClasses.Any(x => x.Type == en.DeclaringType))
				{
					var f = cd.NestClasses.First(nc => nc.Type == en.DeclaringType);
					f.AddEnum(this);
				}
				else
				{
					var ocd = cd.Tree.FindClassDefinitionForType(en.DeclaringType);
                    ocd?.AddEnum(this);
                }
			}
			else
			{
				cd.Tree.AddType(en);
			}
		}

		public Type Type { get; }
		public ClassDefinition Parent { get; set; }
		public Dictionary<int, string> Values = new Dictionary<int, string>();
		public string Name { get; }

		public string Write()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine($"public enum {Name} {{");
			var vals = Values.OrderBy(x => x.Key).ToList();
			for (int i = 0; i < vals.Count; i++)
			{
				var v = vals[i];
				string suffix = i < vals.Count - 1 ? "," : "";
				sb.AppendLine($"\t{v.Value} = {v.Key}{suffix}");
			}
			sb.AppendLine("}");
			return sb.ToString();
		}
	}
}
