using System.Globalization;
using System.Linq;
using Naviam.InternetBank.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using Naviam.InternetBank.Entities;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Naviam.UnitTests.InternetBank
{
    /// <summary>
    ///This is a test class for BankUtilityTest and is intended
    ///to contain all BankUtilityTest Unit Tests
    ///</summary>
    [TestClass]
    public class BankUtilityTest
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
        ///A test for AddCommonHeadersToHttpRequest
        ///</summary>
        [TestMethod]
        public void AddCommonHeadersToHttpRequestTest()
        {
            HttpWebRequest request = null; // TODO: Initialize to an appropriate value
            CookieCollection cookies = null; // TODO: Initialize to an appropriate value
            RequestHeaders headers = null; // TODO: Initialize to an appropriate value
            Uri baseUri = null; // TODO: Initialize to an appropriate value
            string referer = string.Empty; // TODO: Initialize to an appropriate value
            string method = string.Empty; // TODO: Initialize to an appropriate value
            bool followRedirect = false; // TODO: Initialize to an appropriate value
            BankUtility.AddCommonHeadersToHttpRequest(request, cookies, headers, baseUri, referer, method, followRedirect);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for DaysBetween
        ///</summary>
        [TestMethod]
        public void DaysBetweenTest()
        {
            // arrange
            var d1 = DateTime.UtcNow;
            var d2 = DateTime.UtcNow.AddDays(30);
            const int expected = 30;
            
            // act
            var actual = BankUtility.DaysBetween(d1, d2);
            
            // assert
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetAbsoluteUri
        ///</summary>
        [TestMethod]
        public void GetAbsoluteUriTest()
        {
            // arrange
            var baseUri = new Uri("https://www.sbsibank.by");
            const string relativeUri = "login.asp";
            const string expected = "https://www.sbsibank.by/login.asp";

            // act
            var actual = BankUtility.GetAbsoluteUri(baseUri, relativeUri);
            
            // assert
            Assert.AreEqual(expected, actual);
        }

        private static ReportPeriod GetReportPeriod(string id, string startDate, string endDate, bool exist = true)
        {
            return new ReportPeriod
            {
                StartDate = DateTime.Parse(startDate, CultureInfo.InvariantCulture),
                EndDate = DateTime.Parse(endDate, CultureInfo.InvariantCulture),
                Id = id,
                Exists = exist
            };
        }

        [TestMethod]
        public void GetReportsToCreateTest()
        {
            // arrange
            var startDate = DateTime.Parse("04/10/2011", CultureInfo.InvariantCulture);
            var endDate = DateTime.Parse("01/01/2012", CultureInfo.InvariantCulture);
            const int maxDaysPeriod = 170;
            var expected = new List<ReportPeriod>
                            {
                                GetReportPeriod(null, "04/10/2011", "09/27/2011", false),
                                GetReportPeriod(null, "09/28/2011", "01/01/2012", false)
                            };

            // act
            var actual = BankUtility.GetReportsToCreate(startDate, endDate, maxDaysPeriod).ToList();

            // assert
            CollectionAssert.AreEquivalent(expected, actual);
        }

        /// <summary>
        ///A test for GetReportPeriods
        ///</summary>
        [TestMethod]
        public void GetReportPeriodsTest()
        {
            // arrange
            var startDate = DateTime.Parse("04/10/2011", CultureInfo.InvariantCulture);
            var endDate = DateTime.Parse("01/01/2012", CultureInfo.InvariantCulture);
            const int maxDaysPeriod = 170;
            var pregeneratedReports = new List<ReportPeriod>
                                        {
                                            GetReportPeriod("1", "02/20/2011", "03/01/2011"),
                                            GetReportPeriod("2", "05/01/2011", "06/01/2011"),
                                            GetReportPeriod("2", "05/10/2011", "06/02/2011"),
                                            GetReportPeriod("2", "09/01/2011", "10/30/2011"),
                                            GetReportPeriod("3", "10/11/2011", "11/03/2011"),
                                            GetReportPeriod("4", "10/17/2011", "02/03/2011"),
                                            GetReportPeriod("5", "10/11/2011", "01/03/2012")
                                        };
            var expected = new List<ReportPeriod>
                            {
                                GetReportPeriod("", "04/10/2011", "05/01/2011"),
                                GetReportPeriod("", "04/10/2011", "05/01/2011"),
                            };
            
            // act
            var actual = BankUtility.GetReportPeriods(startDate, endDate, maxDaysPeriod, pregeneratedReports);
            
            // assert
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetRequest
        ///</summary>
        [TestMethod]
        public void GetRequestTest()
        {
            InetBankRequest request = null; // TODO: Initialize to an appropriate value
            CookieCollection cookies = null; // TODO: Initialize to an appropriate value
            Uri baseUri = null; // TODO: Initialize to an appropriate value
            RequestHeaders headers = null; // TODO: Initialize to an appropriate value
            bool followRedirect = false; // TODO: Initialize to an appropriate value
            string method = string.Empty; // TODO: Initialize to an appropriate value
            HttpWebRequest expected = null; // TODO: Initialize to an appropriate value
            HttpWebRequest actual;
            actual = BankUtility.GetRequest(request, cookies, baseUri, headers, followRedirect, method);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}