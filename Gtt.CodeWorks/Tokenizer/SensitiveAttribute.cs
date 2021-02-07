using System;

namespace Gtt.CodeWorks.Tokenizer
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class SensitiveAttribute : Attribute
    {
        public Reveal Reveal { get; set; }
        public int FirstChars { get; set; }
        public int LastChars { get; set; }

        public SensitiveAttribute(Reveal reveal = Reveal.FirstLast, int firstChars = 1, int lastChars = 1)
        {
            Reveal = reveal;
            FirstChars = firstChars;
            LastChars = lastChars;
        }
    }
}