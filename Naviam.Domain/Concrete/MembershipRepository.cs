using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Naviam.DAL;
using Naviam.Data;
using System.Data.SqlClient;

namespace Naviam.Domain.Concrete
{
    public class MembershipRepository
    {
        public const string DEFAULT_COMPANY_NAME = "Домашняя бухгалтерия";
        public const string DEFAULT_ACCOUNT_NAME = "Кошелёк";

        public IEnumerable<Company> GetCompanies(int? userId)
        {
            return CompaniesDataAdapter.GetCompanies(userId).AsEnumerable();
        }

        public virtual UserProfile GetUser(string userName, string password) { return GetUser(userName, password, false); }
        public virtual UserProfile GetUser(string userName, string password, bool extAuth)
        {
            var profile = MembershipDataAdapter.GetUser(userName, password);
            if (!extAuth && profile != null && !SimpleHash.VerifyHash(userName + password + "SCEX", "SHA512", profile.Password))
                profile = null;
            if (profile != null)
            {
                profile.Companies = GetCompanies(profile.Id);
            }
            return profile;
        }

        public virtual UserProfile GetUserByAccount(int accountId) { return GetUserByAccount(accountId, false); }
        public virtual UserProfile GetUserByAccount(int accountId, bool extAuth)
        {
            var profile = MembershipDataAdapter.GetUserByAccount(accountId);
            if (profile != null)
            {
                profile.Companies = GetCompanies(profile.Id);
            }
            return profile;
        }

        public virtual string SetSessionForUser(UserProfile profile)
        {
            // generate session key
            var cId = Guid.NewGuid().ToString();
            new CacheWrapper().Set(cId, profile, true, null);
            return cId;
        }

        public virtual UserProfile CreateUser(string email, string password)
        {
            return CreateUser(email, password, DEFAULT_COMPANY_NAME, DEFAULT_ACCOUNT_NAME);
        }

        public virtual bool Approve(string email)
        {
            return MembershipDataAdapter.ApproveUser(email);
        }

        public virtual UserProfile CreateUser(string email, string password, string default_company_name, string default_account_name)
        {
            password = SimpleHash.ComputeHash(email.ToLower() + password + "SCEX", "SHA512", null);

            UserProfile res = null;
            res = MembershipDataAdapter.CreateUser(email, password, default_company_name, default_account_name, GetApproveCode());

            return res;
        }

        public string GetApproveCode()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
        }
    }
}
