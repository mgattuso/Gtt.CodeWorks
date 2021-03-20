using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gtt.CodeWorks
{
    public class UserInformation
    {
        public string Username { get; set; }
        public string UserIdentifier { get; set; }
        public Dictionary<string, object> Claims { get; set; }
        public string[] Roles { get; set; }
        public DateTimeOffset? Expiration { get; set; }
        public DateTimeOffset AuthenticatedAt { get; set; }
    }

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
