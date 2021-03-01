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

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> errors = new Dictionary<string, object>();
            List<string> globalErrors = GlobalErrors ?? new List<string>();

            if (MemberErrors.Any())
            {
                foreach (var error in MemberErrors)
                {
                    if (!error.Members.Any())
                    {
                        globalErrors.Add(error.ErrorMessage);
                    }
                    else
                    {
                        foreach (var member in error.Members)
                        {
                            errors.AddOrAppendValue(member, error.ErrorMessage, forceArray: true);
                        }
                    }
                }
            }

            errors[""] = globalErrors.ToArray();
            return errors;
        }
    }
}