namespace Gtt.CodeWorks
{
    public static class ServiceResponseExtensions
    {
        public static bool IsSuccessful(this ServiceResponse response)
        {
            if (response?.MetaData == null) return false;

            return response.MetaData.Result.Outcome() == ResultOutcome.Successful;
        }

        public static bool Is(this ServiceResponse response, ServiceResult result)
        {
            if (response?.MetaData == null) return false;
            return response.MetaData.Result == result;
        }
    }
}