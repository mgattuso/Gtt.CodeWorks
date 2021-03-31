namespace Gtt.CodeWorks.Duplicator
{
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
        public bool IsOverride { get; set; }

    }
}