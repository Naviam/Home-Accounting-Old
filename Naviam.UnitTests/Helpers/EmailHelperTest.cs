using Naviam.WebUI.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;

namespace Naviam.UnitTests.Helpers
{
    
    
    /// <summary>
    ///This is a test class for EmailHelperTest and is intended
    ///to contain all EmailHelperTest Unit Tests
    ///</summary>
    [TestClass]
    public class EmailHelperTest
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
        ///A test for SendSmsAlert
        ///</summary>
        [TestMethod]
        public void SendSmsAlertTest()
        {
            // arrange
            const string message = @"
4..7983
Cash
Uspeshno
2011-10-13 11:54:07
Summa: 400000 BYR
Ostatok: 1200962 BYR
Na vremya: 11:54:12
BLR/MINSK/EUROSET RKC 3
";
            var subject = "BelSwissBank";
            var recipients = "v.hatalski@gmail.com";

            // act
            var result = EmailHelper.SendSmsAlert(subject, recipients, message);

            // assert
            Assert.IsTrue(result);
        }
    }
}
