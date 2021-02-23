namespace Gtt.CodeWorks.Services
{
    public class UserResolverResult
    {

        public UserResolverResult(UserAuthStatus status, UserInformation user = null)
        {
            Status = status;
            User = user;
        }

        public UserInformation User { get; }
        public UserAuthStatus Status { get; }
    }
}