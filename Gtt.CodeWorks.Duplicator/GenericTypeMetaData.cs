using System.Collections.Generic;

namespace Gtt.CodeWorks.Duplicator
{
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
}