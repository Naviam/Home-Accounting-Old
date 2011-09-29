using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Configuration.Provider;

namespace Naviam.Domain.Concrete.Providers
{
    public class NaviamRoleProvider: RoleProvider
    {
        private string _appName;

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new ProviderException("not supported");
        }

        public override string ApplicationName
        {
            get
            {
                return _appName;
            }
            set
            {
                _appName = value;
            }
        }

        public override void CreateRole(string roleName)
        {
            throw new ProviderException("not supported");
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new ProviderException("not supported");
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new ProviderException("not supported");
        }

        public override string[] GetAllRoles()
        {
            throw new ProviderException("not supported");
        }

        public override string[] GetRolesForUser(string username)
        {
            if (username == null)
            {
                throw new ArgumentNullException("username");
            }
            username = username.Trim();
            var res = RoleRepository.GetInstance().GetRolesForUser(username);
            return res != null ? res.ToArray() : new string[0];
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            if (username == null)
            {
                throw new ArgumentNullException("username");
            }
            username = username.Trim();
            if (roleName == null)
            {
                throw new ArgumentNullException("roleName");
            }
            roleName = roleName.Trim();
            if (username.Length < 1)
            {
                return false;
            }
            return RoleRepository.GetInstance().IsUserInRole(username, roleName);
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new ProviderException("not supported");
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new ProviderException("not supported");
        }

        public override bool RoleExists(string roleName)
        {
            throw new ProviderException("not supported");
        }
    }
}
