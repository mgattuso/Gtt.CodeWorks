using System;

namespace Gtt.CodeWorks
{
    public class EnumDetails
    {
        public EnumDetails(Enum e)
        {
            Name = e.ToString();
            Description = e.ToDescription();
            Value = Convert.ToInt32(e);
        }

        public string Name { get; }
        public string Description { get; }
        public int Value { get; }
    }
}