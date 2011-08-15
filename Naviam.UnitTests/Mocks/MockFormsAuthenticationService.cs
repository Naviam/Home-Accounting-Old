using Naviam.WebUI.Controllers;

namespace Naviam.UnitTests.Mocks
{
    public class MockFormsAuthenticationService : IFormsAuthentication
    {
        public void SignIn(string userName, bool createPersistentCookie)
        {
        }

        public void SignOut()
        {
        }
    }
}
