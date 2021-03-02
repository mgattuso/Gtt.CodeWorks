using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Gtt.CodeWorks.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class RegexForCollectionAttribute : ValidationAttribute
    {
        private readonly string _pattern;

        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        public RegexForCollectionAttribute(string pattern, string errorMessage = null) : base(errorMessage)
        {
            _pattern = pattern;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (!(value is ICollection coll)) return ValidationResult.Success;
            ICollectionPropertyNamingStrategy cpns = validationContext.GetService(typeof(ICollectionPropertyNamingStrategy)) as ICollectionPropertyNamingStrategy;
            string message = "";
            int idx = 0;
            var members = new List<string>();
            foreach (var c in coll)
            {
                RegularExpressionAttribute attr = new RegularExpressionAttribute(_pattern);
                var res = attr.GetValidationResult(c, validationContext);
                var invalid = !string.IsNullOrWhiteSpace(res?.ErrorMessage);
                if (invalid)
                {
                    message = res.ErrorMessage;
                    if (cpns != null)
                    {
                        members.Add(cpns.CreateName(validationContext.MemberName, idx, ""));
                    }
                    else
                    {
                        members.Add($"{validationContext.MemberName}[{idx}]");
                    }
                }
                idx++;
            }

            if (members.Any())
            {
                return new ValidationResult(message, members);
            }
            return ValidationResult.Success;
        }
    }
}
