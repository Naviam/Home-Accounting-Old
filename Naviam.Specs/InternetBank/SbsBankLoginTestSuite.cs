using NUnit.Framework;
using Naviam.InternetBank;

namespace Naviam.Specs.InternetBank
{
    [TestFixture]
    public class SbsBank1LoginTestSuite
    {
        private BankClient _client;

        [SetUp]
        public void Init()
        {
            _client = new BankClient(15);
        }

        [TestCase(TestName = "Login with incorrect credentials")]
        public void Test1LoginIncorrectCredentials()
        {
            // arrange
            const string userName = "test";
            const string password = "test";

            // act
            var response = _client.Login(userName, password);

            // assert
            Assert.Equals(response.ErrorCode, 3);
            Assert.False(response.IsAuthenticated);
        }

        [TestCase(TestName = "Login with correct credentials")]
        public void Test2LoginCorrectCredentials()
        {
            // arrange
            const string userName = "FQ529";
            const string password = "XUI4K";

            // act
            var response = _client.Login(userName, password);

            // assert
            Assert.True(response.IsAuthenticated);
        }

        [TearDown]
        public void Clean()
        {
            _client.Logout(true);
        }
    }
}
