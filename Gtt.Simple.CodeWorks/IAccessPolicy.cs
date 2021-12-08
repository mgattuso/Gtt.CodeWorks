namespace Gtt.Simple.CodeWorks
{
    public interface IAccessPolicy
    {
        bool IsAuthorized(UserInformation user);
    }
}
