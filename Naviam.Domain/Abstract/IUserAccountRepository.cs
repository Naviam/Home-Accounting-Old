using System.Collections.Generic;
using Naviam.Data;
using Naviam.Entities.User;

namespace Naviam.Domain.Abstract
{
    public interface IUserAccountRepository
    {
        IEnumerable<Company> GetCompanies(int? userId);

        UserProfile GetUserProfile(string userName, string password);

        void SignInUser(string email, string password);

        void ValidateUser(string email, string password);

        void ForgotPassword(string email);
    }
}
