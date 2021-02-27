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
        private readonly ICollectionPropertyNamingStrategy _collectionPropertyNamingStrategy = new DottedNumberCollectionPropertyNamingStrategy();



        /// <summary>
        /// Creates an instance of the RequestValidator using the .net Validation framework as part of the DataAnnotations.
        /// The collection naming strategy can be overridden by creating a resolvable instance of <code>ICollectionPropertyNamingStrategy</code>
        /// in the serviceProvider
        /// </summary>
        /// <param name="serviceProvider"></param>
        public DataAnnotationsRequestValidator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var altStrategy = serviceProvider.GetService(typeof(ICollectionPropertyNamingStrategy));
            if (altStrategy != null)
            {
                _collectionPropertyNamingStrategy = (ICollectionPropertyNamingStrategy)altStrategy;
            }
        }

        public ValidationAttempt Validate<T>(T request, string prefix = null)
        {
            var context = new ValidationContext(request, serviceProvider: _serviceProvider, items: null);
            var results = new List<ValidationResult>();
            var isValid = new DataAnnotationsValidator(_collectionPropertyNamingStrategy).TryValidateObjectRecursive(request, results, context);


            if (isValid)
            {
                return ValidationAttempt.Success;
            }

            prefix = !string.IsNullOrWhiteSpace(prefix) ? prefix.Trim()+"." : "";

            var ve = ValidationErrorResponse.Member(results.Select(x => new ValidationErrorData()
            {
                ErrorMessage = x.ErrorMessage,
                Members = x.MemberNames.Select(m => ToCamelCase(prefix+m))
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
