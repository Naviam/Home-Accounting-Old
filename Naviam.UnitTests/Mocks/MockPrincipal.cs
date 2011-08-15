using System.Security.Principal;

namespace Naviam.UnitTests.Mocks
{
    public class MockPrincipal : IPrincipal
    {
        IIdentity _identity;

        public IIdentity Identity
        {
            get
            {
                if (_identity == null)
                {
                    _identity = new MockIdentity();
                }
                return _identity;
            }
        }

        public bool IsInRole(string role)
        {
            return false;
        }
    }
}
