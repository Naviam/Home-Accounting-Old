using Naviam.WebUI.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Naviam.UnitTests
{
    /// <summary>
    ///This is a test class for SmsControllerTest and is intended
    ///to contain all SmsControllerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SmsControllerTest
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
        ///A test for RecieveMessage
        ///</summary>
        [TestMethod]
        public void RecieveMessageTest()
        {
            // arrange
            var target = new SmsController();
            const string key = "givemeaccesstotoyou"; 
            const string gateway = "GETWAY1";
            const string @from = "BelSwissBank";
            var to = string.Empty; 
            const string message = @"4..3855
ATM
Uspeshno
2011-10-15 13:38:09
Summa: 200000 BYR
Ostatok: 333887 BYR
Na vremya: 13:38:16
BLR/MINSK/ATMMMB HO19 CUM MINSK";

            // act
            var actual = target.RecieveMessage(key, gateway, from, to, message);
            
            // assert
            Assert.AreEqual("ok", actual.Data);
        }
    }
}
