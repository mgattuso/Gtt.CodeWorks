using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

namespace Gtt.CodeWorks.DataAnnotations
{
    /// <summary>
    /// Ensures a collection is present and populated with at least one or the specified number of entries
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public sealed class ValidEnumDefinedAttribute : ValidationAttribute
    {
        public ValidEnumDefinedAttribute() : base("Value not found in enum")
        {

        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;
            Type t = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();
            if (!Enum.IsDefined(t, value))
            {
                var list = Enum.GetNames(t);
                return new ValidationResult($"Expected one of: {string.Join(",", list)}");
            }

            return ValidationResult.Success;
        }
    }
}
