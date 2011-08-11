using System.Collections.Generic;
using System.Linq;
using Naviam.DAL;
using Naviam.Data;
using Naviam.Domain.Abstract;
using Naviam.Entities.User;

namespace Naviam.Domain.Concrete
{
    public class UserAccountRepository : IUserAccountRepository
    {
        public IEnumerable<Company> GetCompanies(int? userId)
        {
            return CompaniesDataAdapter.GetCompanies(userId).AsEnumerable();
        }

        public UserProfile GetUserProfile(string userName, string password)
        {
            var profile = UserDataAdapter.GetUserProfile(userName, password);
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
        
        public void ValidateUser(string email, string password)
        {
            throw new System.NotImplementedException();
        }


        public void SignInUser(string email, string password)
        {
            throw new System.NotImplementedException();
        }

        public void ForgotPassword(string email)
        {
            throw new System.NotImplementedException();
        }

        
    }
}
