using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Naviam.DAL;
using Naviam.Data;

namespace Naviam.Domain.Concrete
{
    public class MembershipRepository
    {
        public IEnumerable<Company> GetCompanies(int? userId)
        {
            return CompaniesDataAdapter.GetCompanies(userId).AsEnumerable();
        }

        public virtual UserProfile GetUser(string userName, string password) { return GetUser(userName, password, false); }
        public virtual UserProfile GetUser(string userName, string password, bool extAuth)
        {
            var profile = MembershipDataAdapter.GetUser(userName, password);
            if (!extAuth && !SimpleHash.VerifyHash(userName + password + "SCEX", "SHA512", profile.Password))
                profile = null;

            if (profile != null)
            {

                //TODO: read companies and attach to user, also assign default company
                profile.Companies = GetCompanies(profile.Id);
                profile.DefaultCompany = 1;
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
    }
}
