using System.Collections.Generic;
using System.Linq;
using System.Web;
using Naviam.DAL;
using Naviam.Data;
using Naviam.Entities.User;

namespace Naviam.Domain.Concrete
{
    public class MembershipRepository
    {
        public IEnumerable<Company> GetCompanies(int? userId)
        {
            return CompaniesDataAdapter.GetCompanies(userId).AsEnumerable();
        }

        public virtual UserProfile GetUser(string userName, string password)
        {
            var profile = MembershipDataAdapter.GetUser(userName, password);
            if (!SimpleHash.VerifyHash(userName + password + "SCEX", "SHA512", profile.Password))
                profile = null;

            if (profile != null)
            {
                //TODO: read companies and attach to user, also assign default company
                profile.Companies = GetCompanies(profile.Id);
                profile.DefaultCompany = 1;
            }
            return profile;
        }
    }
}
