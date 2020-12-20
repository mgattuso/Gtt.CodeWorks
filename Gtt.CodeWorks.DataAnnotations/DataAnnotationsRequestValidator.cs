using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gtt.CodeWorks.Validation;

namespace Gtt.CodeWorks.DataAnnotations
{
    public class DataAnnotationsRequestValidator : IRequestValidator
    {
        private readonly IServiceProvider _serviceProvider;

        public DataAnnotationsRequestValidator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ValidationAttempt Validate<T>(T request) where T : BaseRequest
        {
            var context = new ValidationContext(request, serviceProvider: _serviceProvider, items: null);
            var results = new List<ValidationResult>();
            var isValid = new DataAnnotationsValidator().TryValidateObjectRecursive(request, results, context);


            if (isValid)
            {
                return ValidationAttempt.Success;
            }

            var ve = ValidationErrorResponse.Member(results.Select(x => new ValidationErrorData()
            {
                ErrorMessage = x.ErrorMessage,
                Members = x.MemberNames.Select(ToCamelCase)
            }).ToArray());
            return ValidationAttempt.Unsuccessful(ve);
        }

        private static string ToCamelCase(string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }
            return str;
        }
    }
}
