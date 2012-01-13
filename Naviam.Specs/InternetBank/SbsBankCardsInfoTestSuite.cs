using System;
using System.Linq;
using NUnit.Framework;
using Naviam.InternetBank;

namespace Naviam.Specs.InternetBank
{
    [TestFixture]
    public class SbsBank2CardsInfoTestSuite
    {
        private BankClient _client;

        [SetUp]
        public void Init()
        {
            _client = new BankClient(15);
            const string userName = "FQ529";
            const string password = "XUI4K";
            _client.Login(userName, password);
        }

        [TestCase]
        public void Test1PaymentCards()
        {
            // arrange

            // act
            var cards = _client.GetPaymentCards();
            // assert
            Assert.IsNotNull(cards);
            Assert.IsTrue(cards.Any());
        }

        [TestCase]
        public void Test2CardTransactions()
        {
            // arrange
            var card = _client.GetPaymentCards().FirstOrDefault();
            const int daysToParse = 7;

            // act
            var transactions = 
                _client.GetTransactions(card, DateTime.UtcNow.AddDays(-daysToParse));

            // assert
            Assert.IsNotNull(transactions);
            Assert.IsTrue(transactions.Any());
            Assert.GreaterOrEqual(transactions.Min(r => r.OperationDate).Date, DateTime.UtcNow.AddDays(-daysToParse).Date);
        }

        [TearDown]
        public void Clean()
        {
            _client.Logout(true);
        }
    }
}
