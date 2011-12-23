using NUnit.Framework;
using Naviam.InternetBank;

namespace Naviam.Specs.InternetBank
{
    [TestFixture]
    public class SbsBankLoginTestSuite
    {
        private BankClient _client;

        [SetUp]
        public void Init()
        {
            _client = new BankClient(15);
        }

        [TestCase]
        public void LoginIncorrectCredentials()
        {
            // arrange
            const string userName = "test";
            const string password = "test";

            // act
            var authenticated = _client.Login(userName, password);

            // assert
            Assert.False(authenticated.IsAuthenticated);
        }

        [TestCase]
        public void LoginCorrectCredentials()
        {
            // arrange
            const string userName = "FQ529";
            const string password = "XUI4K";

            // act
            var authenticated = _client.Login(userName, password);

            // assert
            Assert.True(authenticated.IsAuthenticated);
        }

        [TearDown]
        public void Clean()
        {
            _client.Logout(true);
        }
    }
}
