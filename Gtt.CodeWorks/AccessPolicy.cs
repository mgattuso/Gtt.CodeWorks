using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gtt.CodeWorks
{
    public interface IAccessPolicy
    {
        bool IsAuthorized(UserInformation user);
    }

    public class CustomAccessPolicy : IAccessPolicy
    {
        private readonly Func<UserInformation, bool> _selection;

        public CustomAccessPolicy(Func<UserInformation, bool> selection)
        {
            _selection = selection;
        }
        public bool IsAuthorized(UserInformation user)
        {
            if (user == null) return false;
            if (_selection == null) return false;
            var r = _selection(user);
            return r;
        }
    }

    public class LoggedInAccessPolicy : IAccessPolicy
    {
        public virtual bool IsAuthorized(UserInformation user)
        {
            if (user == null) return false;
            var now = ServiceClock.CurrentTime();
            return (user.Expiration == null || user.Expiration < now);
        }
    }

    public class InRoleAccessPolicy : LoggedInAccessPolicy
    {
        private readonly string[] _roles;

        public InRoleAccessPolicy(params string[] roles)
        {
            _roles = (roles ?? new string[0]).Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.ToLowerInvariant()).ToArray();
        }
        public override bool IsAuthorized(UserInformation user)
        {
            var loggedIn = base.IsAuthorized(user);
            if (!loggedIn) return false;
            if (user.Roles == null || user.Roles.Length == 0) return false;
            var r = _roles.Intersect(user.Roles).Any();
            return r;
        }
    }

    public class InternalOnlyAccessPolicy : IAccessPolicy
    {
        private readonly bool _allowAccessInNonProd;
        private readonly CodeWorksEnvironment _environment;

        public InternalOnlyAccessPolicy(bool allowAccessInNonProd, CodeWorksEnvironment environment)
        {
            _allowAccessInNonProd = allowAccessInNonProd;
            _environment = environment;
        }
        public bool IsAuthorized(UserInformation user)
        {
            if (_environment == CodeWorksEnvironment.Production || !_allowAccessInNonProd)
            {
                // THE DEFAULT MIDDLEWARE WILL BYPASS THE AUTHORIZATION
                // ALLOWING IT TO BE EXECUTED
                return false;
            }
            // THE DEFAULT 
            return true;
        }
    }

    public class AllowAnonymousAccessPolicy : IAccessPolicy
    {
        public bool IsAuthorized(UserInformation user)
        {
            return true;
        }
    }
}
