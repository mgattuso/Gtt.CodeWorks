namespace Gtt.CodeWorks.Validation
{
    public interface IRequestValidator
    {
        ValidationAttempt Validate<T>(T request) where T : BaseRequest;
    }
}
