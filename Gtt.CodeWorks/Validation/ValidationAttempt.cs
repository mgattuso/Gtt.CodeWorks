using System.Collections.Generic;

namespace Gtt.CodeWorks.Validation
{
    public class ValidationAttempt
    {
        private readonly string _member;
        private readonly string _error;

        private ValidationAttempt(bool isValid, string member = null, string error = null)
        {
            _member = member;
            _error = error;
            IsValid = isValid;
        }

        public bool IsValid { get; }

        public static ValidationAttempt Success => new ValidationAttempt(true);

        public static ValidationAttempt Unsuccessful(string member, string error)
        {
            return new ValidationAttempt(false, member, error);
        }

        public static ValidationAttempt Unsuccessful(string globalError)
        {
            return Unsuccessful("", globalError);
        }
    }
}