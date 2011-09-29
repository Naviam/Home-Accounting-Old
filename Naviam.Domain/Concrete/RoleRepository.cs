using System;
using System.Configuration.Provider;
using Naviam.DAL;
using Naviam.Domain.Entities;
using System.Collections.Generic;

namespace Naviam.Domain.Concrete
{
    public class RoleRepository
    {
        private static RoleRepository _instance;

        private RoleRepository()
        {
        }

        public static RoleRepository GetInstance()
        {
            return _instance ?? (_instance = new RoleRepository());
        }

        public void CreateRole(string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 0x100, "roleName");

            var returnResult = RoleDataAdapter.CreateRole(roleName);

            switch (returnResult)
            {
                case 0: return;
                case 1: throw new ProviderException("Role already exists");
            }
            throw new ProviderException("Unknown failure");
        }

        public bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 0x100, "roleName");
            var returnValue = RoleDataAdapter.DeleteRole(roleName, throwOnPopulatedRole);

            if (returnValue == 2)
            {
                throw new ProviderException("Role is not empty");
            }

            return returnValue == 0;
        }

        public string[] GetAllRoles()
        {
            return RoleDataAdapter.GetAllRoles();
        }

        public List<string> GetRolesForUser(string username) { return GetRolesForUser(username, false); }
        public List<string> GetRolesForUser(string username, bool forceSqlLoad)
        {
            if (username.Length < 1)
            {
                return null;
            }
            var cache = new CacheWrapper();
            var res = cache.GetList<string>("user_roles_" + username);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                res = RoleDataAdapter.GetRolesForUser(username);
                //save to cache
                cache.SetList("user_roles_" + username, res);
            }
            return res;
        }

        public string[] GetUsersInRole(string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 0x100, "roleName");

            int returnValue;
            var users = RoleDataAdapter.GetUsersInRole(roleName, out returnValue);
            if (users.Length < 1)
            {
                switch (returnValue)
                {
                    case 0: return new string[0];
                    case 1: throw new ProviderException("Role has not been found");
                }
                throw new ProviderException("Unknown Failure");
            }
            return users;
        }

        public bool IsUserInRole(string username, string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 0x100, "roleName");
            SecUtility.CheckParameter(ref username, true, false, true, 0x100, "username");
            if (username.Length < 1)
            {
                return false;
            }
            var resultValue = RoleDataAdapter.IsUserInRole(username, roleName);
            switch (resultValue)
            {
                case 0: return false;
                case 1: return true;
                case 2: return false;
                case 3: return false;
            }
            throw new ProviderException("Unknown Failure");
        }

        public string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 0x100, "roleName");
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, 0x100, "usernameToMatch");

            int returnValue;
            var users = RoleDataAdapter.FindUsersInRole(roleName, usernameToMatch, out returnValue);

            if (users.Length < 1)
            {
                switch (returnValue)
                {
                    case 0: return new string[0];
                    case 1: throw new ProviderException("Role has not been found");
                }
                throw new ProviderException("Unknown failure");
            }

            return users;
        }

        public void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            SecUtility.CheckArrayParameter(ref roleNames, true, true, true, 0x100, "roleNames");
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, 0x100, "usernames");
            int resultValue;
            RoleDataAdapter.AddUsersToRoles(String.Join(",", usernames), String.Join(",", roleNames), out resultValue);

            switch (resultValue)
            {
                case 0: return;
                case 1: throw new ProviderException("This user has not been found");
                case 2: throw new ProviderException("Role has not been found");
                case 3: throw new ProviderException("This user is already in role");
            }
        }

        public void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            SecUtility.CheckArrayParameter(ref roleNames, true, true, true, 0x100, "roleNames");
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, 0x100, "usernames");
            int resultValue;
            RoleDataAdapter.RemoveUsersFromRoles(String.Join(",", usernames), String.Join(",", roleNames), out resultValue);

            switch (resultValue)
            {
                case 0: return;
                case 1: throw new ProviderException("This user has not been found");
                case 2: throw new ProviderException("Role has not been found");
                case 3: throw new ProviderException("This user is not in role");
            }
            throw new ProviderException("Unknown failure");
        }

        public bool RoleExists(string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 0x100, "roleName");
            var returnValue = RoleDataAdapter.RoleExists(roleName);

            switch (returnValue)
            {
                case 0: return false;
                case 1: return true;
            }
            throw new ProviderException("Unknown failure");
        }
    }
}
