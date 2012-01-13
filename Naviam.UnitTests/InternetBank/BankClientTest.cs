using Naviam.InternetBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Naviam.InternetBank.Entities;
using System.Collections.Generic;

namespace Naviam.UnitTests.InternetBank
{
    /// <summary>
    ///This is a test class for BankClientTest and is intended
    ///to contain all BankClientTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BankClientTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for BankClient Constructor
        ///</summary>
        [TestMethod()]
        public void BankClientConstructorTest()
        {
            int bankId = 0; // TODO: Initialize to an appropriate value
            BankClient target = new BankClient(bankId);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod()]
        public void DisposeTest()
        {
            int bankId = 0; // TODO: Initialize to an appropriate value
            BankClient target = new BankClient(bankId); // TODO: Initialize to an appropriate value
            target.Dispose();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for GetPaymentCards
        ///</summary>
        [TestMethod()]
        public void GetPaymentCardsTest()
        {
            int bankId = 0; // TODO: Initialize to an appropriate value
            BankClient target = new BankClient(bankId); // TODO: Initialize to an appropriate value
            IEnumerable<PaymentCard> expected = null; // TODO: Initialize to an appropriate value
            IEnumerable<PaymentCard> actual;
            actual = target.GetPaymentCards();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetTransactions
        ///</summary>
        [TestMethod()]
        public void GetTransactionsTest()
        {
            int bankId = 0; // TODO: Initialize to an appropriate value
            BankClient target = new BankClient(bankId); // TODO: Initialize to an appropriate value
            PaymentCard card = null; // TODO: Initialize to an appropriate value
            DateTime startDate = new DateTime(); // TODO: Initialize to an appropriate value
            IEnumerable<AccountTransaction> expected = null; // TODO: Initialize to an appropriate value
            IEnumerable<AccountTransaction> actual;
            actual = target.GetTransactions(card, startDate);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for LoadBanks
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Naviam.InternetBank.dll")]
        public void LoadBanksTest()
        {
            string fileName = string.Empty; // TODO: Initialize to an appropriate value
            IEnumerable<InetBank> expected = null; // TODO: Initialize to an appropriate value
            IEnumerable<InetBank> actual;
            actual = BankClient_Accessor.LoadBanks(fileName);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for LoadSettings
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Naviam.InternetBank.dll")]
        public void LoadSettingsTest()
        {
            string sbsibanksettingsXml = string.Empty; // TODO: Initialize to an appropriate value
            InetBankSettings expected = null; // TODO: Initialize to an appropriate value
            InetBankSettings actual;
            actual = BankClient_Accessor.LoadSettings(sbsibanksettingsXml);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Login
        ///</summary>
        [TestMethod()]
        public void LoginTest()
        {
            int bankId = 0; // TODO: Initialize to an appropriate value
            BankClient target = new BankClient(bankId); // TODO: Initialize to an appropriate value
            string userName = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            LoginResponse expected = null; // TODO: Initialize to an appropriate value
            LoginResponse actual;
            actual = target.Login(userName, password);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Logout
        ///</summary>
        [TestMethod()]
        public void LogoutTest()
        {
            int bankId = 0; // TODO: Initialize to an appropriate value
            BankClient target = new BankClient(bankId); // TODO: Initialize to an appropriate value
            bool cleanAuthCookies = false; // TODO: Initialize to an appropriate value
            target.Logout(cleanAuthCookies);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for InetBank
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Naviam.InternetBank.dll")]
        public void InetBankTest()
        {
            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            BankClient_Accessor target = new BankClient_Accessor(param0); // TODO: Initialize to an appropriate value
            InetBank expected = null; // TODO: Initialize to an appropriate value
            InetBank actual;
            target.InetBank = expected;
            actual = target.InetBank;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Settings
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Naviam.InternetBank.dll")]
        public void SettingsTest()
        {
            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            BankClient_Accessor target = new BankClient_Accessor(param0); // TODO: Initialize to an appropriate value
            InetBankSettings expected = null; // TODO: Initialize to an appropriate value
            InetBankSettings actual;
            target.Settings = expected;
            actual = target.Settings;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
