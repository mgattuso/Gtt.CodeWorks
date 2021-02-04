using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Gtt.CodeWorks.DataAnnotations
{
    public class DataAnnotationsValidator
    {
        private readonly ICollectionPropertyNamingStrategy _collectionValidationNaming;

        public DataAnnotationsValidator(ICollectionPropertyNamingStrategy collectionValidationNaming)
        {
            _collectionValidationNaming = collectionValidationNaming ?? throw new ArgumentNullException(nameof(collectionValidationNaming));
        }

        private bool TryValidateObject(object obj, ICollection<ValidationResult> results, IServiceProvider serviceProvider, IDictionary<object, object> validationContextItems = null)
        {
            return Validator.TryValidateObject(obj, new ValidationContext(obj, serviceProvider, validationContextItems), results, true);
        }

        public bool TryValidateObjectRecursive<T>(T obj, List<ValidationResult> results, ValidationContext context)
        {
            return TryValidateObjectRecursive(obj, results, context, new HashSet<object>());
        }

        private bool TryValidateObjectRecursive<T>(T obj, List<ValidationResult> results, ValidationContext context, ISet<object> validatedObjects)
        {
            //short-circuit to avoid infinite loops on cyclical object graphs
            if (validatedObjects.Contains(obj))
            {
                return true;
            }

            validatedObjects.Add(obj);
            bool result = TryValidateObject(obj, results, context);

            var properties = obj.GetType().GetProperties().Where(prop => prop.CanRead
                                                                         && !prop.GetCustomAttributes(typeof(SkipRecursiveValidation), false).Any()
                                                                         && prop.GetIndexParameters().Length == 0).ToList();

            foreach (var property in properties)
            {
                var alwaysPresent = property.GetCustomAttribute<AlwaysPresentAttribute>();
                if (property.PropertyType.IsValueType)
                {
                    continue;
                }

                if (property.PropertyType == typeof(string))
                {
                    if (alwaysPresent == null) continue;
                    string sv = property.GetValue(obj) as string;
                    if (string.IsNullOrWhiteSpace(sv))
                    {
                        string prop = property.Name;
                        result = false;
                        results.Add(new ValidationResult(alwaysPresent.ErrorMessage ?? $"The {prop} field is required", new [] { property.Name }));
                    }
                }

                var value = GetPropertyValue(obj, property.Name);

                if (value == null) continue;

                if (value is IEnumerable asEnumerable)
                {
                    int counter = 0;
                    foreach (var enumObj in asEnumerable)
                    {
                        if (enumObj != null)
                        {
                            var nestedResults = new List<ValidationResult>();
                            if (!TryValidateObjectRecursive(enumObj, nestedResults, context, validatedObjects))
                            {
                                result = false;
                                foreach (var validationResult in nestedResults)
                                {
                                    PropertyInfo property1 = property;
                                    var counter1 = counter;

                                    var memberNames = validationResult.MemberNames.Select(x => _collectionValidationNaming.CreateName(property1.Name, counter1, x));
                                    results.Add(new ValidationResult(validationResult.ErrorMessage, memberNames));
                                }
                            }
                        }
                        counter++;
                    }
                }
                else
                {
                    var nestedResults = new List<ValidationResult>();
                    if (!TryValidateObjectRecursive(value, nestedResults, context, validatedObjects))
                    {
                        result = false;
                        foreach (var validationResult in nestedResults)
                        {
                            PropertyInfo property1 = property;
                            results.Add(new ValidationResult(validationResult.ErrorMessage, validationResult.MemberNames.Select(x => property1.Name + '.' + x)));
                        }
                    };
                }
            }

            return result;
        }

        private static object GetPropertyValue(object o, string propertyName)
        {
            object objValue = string.Empty;

            var propertyInfo = o.GetType().GetProperty(propertyName);
            if (propertyInfo != null)
                objValue = propertyInfo.GetValue(o, null);

            return objValue;
        }
    }
}