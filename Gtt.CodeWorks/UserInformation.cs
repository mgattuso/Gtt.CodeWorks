using System;
using System.Collections.Generic;
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
}
