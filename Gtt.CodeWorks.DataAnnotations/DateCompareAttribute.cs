using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Gtt.CodeWorks.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DateCompareAttribute : ValidationAttribute
    {
        public string OtherProperty { get; }
        public DateCompareCheck Check { get; }

        public DateCompareAttribute(DateCompareCheck check, string otherProperty) : base("{0} must be {1} {2}")
        {
            Check = check;
            OtherProperty = otherProperty ?? throw new ArgumentNullException(nameof(otherProperty));
        }

        public string OtherPropertyDisplayName { get; internal set; }
        public bool ListAllMembers { get; set; }

        public override bool RequiresValidationContext => true;

        public override string FormatErrorMessage(string name) =>
            string.Format(
                CultureInfo.CurrentCulture, ErrorMessageString, name, Check.ToDescription(), OtherPropertyDisplayName ?? OtherProperty);

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Type[] allowedTypes = { typeof(DateTime), typeof(DateTime?), typeof(DateTimeOffset), typeof(DateTimeOffset?) };

            if (value == null) return ValidationResult.Success;

            if (allowedTypes.Contains(validationContext.GetType()))
            {
                return new ValidationResult("The Property type should be DateTime or DateTimeOffset");
            }

            PropertyInfo otherPropertyInfo = validationContext.ObjectType.GetRuntimeProperty(OtherProperty);
            if (otherPropertyInfo == null)
            {
                return new ValidationResult($"{OtherProperty} not found");
            }
            if (otherPropertyInfo.GetIndexParameters().Any())
            {
                throw new ArgumentException($"{OtherProperty} not found on {validationContext.ObjectType.FullName}");
            }

            if (!allowedTypes.Contains(otherPropertyInfo.PropertyType))
            {
                return new ValidationResult($"The Property type for {OtherProperty} should be DateTime or DateTimeOffset");
            }

            object otherPropertyValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);

            if (otherPropertyValue == null)
            {
                return ValidationResult.Success;
            }

            var isValid = ComparisonIsValid(ConvertToDate(value, value.GetType()), ConvertToDate(otherPropertyValue, otherPropertyInfo.PropertyType));

            if (!isValid)
            {
                if (OtherPropertyDisplayName == null)
                {
                    OtherPropertyDisplayName = GetDisplayNameForProperty(otherPropertyInfo);
                }

                string[] memberNames = validationContext.MemberName != null
                   ? ListAllMembers
                       ? new[] { validationContext.MemberName, OtherProperty }
                       : new[] { validationContext.MemberName }
                   : null;
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), memberNames);
            }

            return null;
        }

        private string GetDisplayNameForProperty(PropertyInfo property)
        {
            var attributes = CustomAttributeExtensions.GetCustomAttributes(property, true);
            var display = attributes.OfType<DisplayAttribute>().FirstOrDefault();
            if (display != null)
            {
                // TODO-NULLABLE: This will return null if [DisplayName] is specified but no Name has been defined - probably a bug.
                // Should fall back to OtherProperty in this case instead.
                return display.GetName();
            }

            return OtherProperty;
        }

        private bool ComparisonIsValid(DateTime thisProperty, DateTime otherProperty)
        {
            switch (Check)
            {
                case DateCompareCheck.Before:
                    return thisProperty < otherProperty;
                case DateCompareCheck.After:
                    return thisProperty > otherProperty;
                case DateCompareCheck.OnOrBefore:
                    return thisProperty <= otherProperty;
                case DateCompareCheck.OnOrAfter:
                    return thisProperty >= otherProperty;
                case DateCompareCheck.Same:
                    return thisProperty == otherProperty;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private DateTime ConvertToDate(object value, Type type)
        {
            if (type == typeof(DateTime))
                return (DateTime)value;
            if (type == typeof(DateTime?))
                return ((DateTime?)value).Value;
            if (type == typeof(DateTimeOffset))
                return ((DateTimeOffset)value).DateTime;
            if (type == typeof(DateTimeOffset?))
                return ((DateTimeOffset?)value).Value.DateTime;
            throw new ArgumentException($"Property type is not a valid datetime type", nameof(type));
        }
    }

    public enum DateCompareCheck
    {
        [Description("before")]
        Before,
        [Description("after")]
        After,
        [Description("on or before")]
        OnOrBefore,
        [Description("on or after")]
        OnOrAfter,
        [Description("the same as")]
        Same
    }
}
