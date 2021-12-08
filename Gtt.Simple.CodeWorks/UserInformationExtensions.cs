using System;
using System.Linq;

namespace Gtt.Simple.CodeWorks
{
    public static class UserInformationExtensions
    {
        public static bool IsInRole(this UserInformation user, string role)
        {
            if (user == null) return false;
            return user.Roles.Contains(role, StringComparer.InvariantCultureIgnoreCase);
        }

        public static bool IsInRole<T>(this UserInformation user, T role) where T : struct, IConvertible
        {
            var rs = role.ToString();
            return IsInRole(user, rs);
        }
    }
}
