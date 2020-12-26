using System;
using System.Collections.Generic;
using System.Linq;

namespace Gtt.CodeWorks.Validation
{
    [Serializable]
    public class ValidationErrorResponse
    {
        public ValidationErrorResponse()
        {
            MemberErrors = new List<ValidationErrorData>();
            GlobalErrors = new List<string>();
        }

        public List<ValidationErrorData> MemberErrors { get; set; }
        public List<string> GlobalErrors { get; set; }

        public void AddValidationError(ValidationErrorData validationError)
        {
            if (validationError == null) return;
            if (validationError.Members.Any())
            {
                MemberErrors.Add(validationError);
            }
            else
            {
                GlobalErrors.Add(validationError.ErrorMessage);
            }
        }

        public void AddValidationError(string member, string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(member))
            {
                GlobalErrors.Add(error);
                return;
            }

            MemberErrors.Add(new ValidationErrorData(error, new[] { member }));
        }

        public static ValidationErrorResponse Global(string error)
        {
            var ve = new ValidationErrorResponse();
            ve.GlobalErrors.Add(error);
            return ve;
        }

        public static ValidationErrorResponse Member(string member, string error)
        {
            var ve = new ValidationErrorResponse();
            ve.AddValidationError(member, error);
            return ve;
        }

        public static ValidationErrorResponse Member(params Tuple<string, string>[] errors)
        {
            var ve = new ValidationErrorResponse();
            foreach (var error in errors)
            {
                ve.AddValidationError(error.Item1, error.Item2);
            }

            return ve;
        }

        public static ValidationErrorResponse Member(params ValidationErrorData[] errors)
        {
            var ve = new ValidationErrorResponse();
            foreach (ValidationErrorData error in errors)
            {
                ve.MemberErrors.Add(error);
            }

            return ve;
        }

        public Dictionary<string, string[]> ToDictionary()
        {
            Dictionary<string, string[]> errors = new Dictionary<string, string[]>();
            if (GlobalErrors.Any())
            {
                errors[""] = GlobalErrors.ToArray();
            }

            if (MemberErrors.Any())
            {
                foreach (var error in MemberErrors)
                {
                    foreach (var member in error.Members)
                    {
                        if (errors.TryGetValue(member, out var memberErrors))
                        {
                            string[] arr = new string[memberErrors.Length + 1];
                            Array.Copy(memberErrors, arr, memberErrors.Length);
                            arr[memberErrors.Length] = error.ErrorMessage;
                            errors[member] = arr;
                        }
                        else
                        {
                            errors[member] = new[] { error.ErrorMessage };
                        }
                    }
                }
            }

            return errors;
        }
    }
}