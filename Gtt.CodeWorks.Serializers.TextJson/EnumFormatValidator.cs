using Newtonsoft.Json.Linq;
using NJsonSchema.Validation;
using NJsonSchema.Validation.FormatValidators;

namespace Gtt.CodeWorks.Serializers.TextJson
{
    public class EnumFormatValidator : IFormatValidator
    {
        public bool IsValid(string value, JTokenType tokenType)
        {
            return true;
        }

        public ValidationErrorKind ValidationErrorKind { get; set; }
        public string Format => "Enum";
    }
}