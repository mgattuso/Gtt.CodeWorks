using System.Reflection;

namespace Gtt.CodeWorks.Tokenizer
{
    public class TokenTracker
    {
        public int Correlation { get; set; }
        public object Root { get; set; }
        public ITokenizable Value { get; set; }
        public PropertyInfo Property { get; set; }
        public SensitiveAttribute SensitiveAttribute { get; set; }

    }
}