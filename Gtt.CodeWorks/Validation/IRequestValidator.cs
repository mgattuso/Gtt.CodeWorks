namespace Gtt.CodeWorks.Validation
{
    public interface IRequestValidator
    {
        ValidationAttempt Validate<T>(T request, string prefix = null);
    }
}
