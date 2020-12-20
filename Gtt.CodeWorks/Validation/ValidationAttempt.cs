namespace Gtt.CodeWorks.Validation
{
    public class ValidationAttempt
    {
        private ValidationAttempt(bool isValid, ValidationErrorResponse errors)
        {
            IsValid = isValid;
            Errors = errors;
        }

        public bool IsValid { get; }
        public ValidationErrorResponse Errors { get; }

        public static ValidationAttempt Success => new ValidationAttempt(true, null);

        public static ValidationAttempt Unsuccessful(ValidationErrorResponse errors)
        {
            return new ValidationAttempt(false, errors);
        }

        public static ValidationAttempt Unsuccessful(string property, string error)
        {
            return Unsuccessful(ValidationErrorResponse.Member(property, error));
        }

        public static ValidationAttempt Unsuccessful(string globalError)
        {
            return Unsuccessful(ValidationErrorResponse.Global(globalError));
        }
    }
}