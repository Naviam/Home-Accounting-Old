using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Naviam.InternetBank;

namespace Naviam.Specs.InternetBank
{
    [TestFixture]
    public class SbsBankCardsInfoTestSuite
    {
        private BankClient _client;

        [SetUp]
        public void Init()
        {
            _client = new BankClient(15);
            const string userName = "FQ529";
            const string password = "XUI4K";
            var authenticated = _client.Login(userName, password);
        }

        [TestCase]
        public void GetPaymentCards()
        {
            // arrange

            // act
            var cards = _client.GetPaymentCards();

            // assert
            Assert.IsNotNull(cards);
            Assert.IsTrue(cards.Any());
        }

        [TearDown]
        public void Clean()
        {
            _client.Logout(true);
        }
    }
}
