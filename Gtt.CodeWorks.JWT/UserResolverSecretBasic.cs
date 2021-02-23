namespace Gtt.CodeWorks.JWT
{
    public class UserResolverSecretBasic : IUserResolverSecret
    {
        private readonly string _secret;

        public UserResolverSecretBasic(string secret)
        {
            _secret = secret;
        }
        public string GetSecret()
        {
            return _secret;
        }
    }
}