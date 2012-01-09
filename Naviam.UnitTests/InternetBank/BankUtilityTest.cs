using System.Globalization;
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

        /// <summary>
        ///A test for GetReportPeriods
        ///</summary>
        [TestMethod]
        public void GetReportPeriodsTest()
        {
            // arrange
            var startDate = DateTime.Parse("10/10/2010", CultureInfo.InvariantCulture);
            var endDate = DateTime.Parse("04/04/2011", CultureInfo.InvariantCulture);
            const int maxDaysPeriod = 100;
            var pregeneratedReports = new List<ReportPeriod>
                                        {
                                            new ReportPeriod
                                                {
                                                      StartDate = DateTime.Parse("10/20/2010", CultureInfo.InvariantCulture),
                                                      EndDate = DateTime.Parse("10/30/2010", CultureInfo.InvariantCulture),
                                                      Id = "1"
                                                },
                                            new ReportPeriod
                                                {
                                                      StartDate = DateTime.Parse("09/01/2010", CultureInfo.InvariantCulture),
                                                      EndDate = DateTime.Parse("10/30/2010", CultureInfo.InvariantCulture),
                                                      Id = "2"
                                                },
                                            new ReportPeriod
                                                {
                                                      StartDate = DateTime.Parse("10/11/2010", CultureInfo.InvariantCulture),
                                                      EndDate = DateTime.Parse("03/03/2011", CultureInfo.InvariantCulture),
                                                      Id = "3"
                                                },
                                            new ReportPeriod
                                                {
                                                      StartDate = DateTime.Parse("10/17/2010", CultureInfo.InvariantCulture),
                                                      EndDate = DateTime.Parse("02/03/2011", CultureInfo.InvariantCulture),
                                                      Id = "4"
                                                },
                                            new ReportPeriod
                                                {
                                                      StartDate = DateTime.Parse("10/11/2011", CultureInfo.InvariantCulture),
                                                      EndDate = DateTime.Parse("01/03/2012", CultureInfo.InvariantCulture),
                                                      Id = "5"
                                                }
                                        };
            IEnumerable<ReportPeriod> expected = new List<ReportPeriod>
                                        {
                                            new ReportPeriod
                                                {
                                                      StartDate = DateTime.Parse("10/20/2010", CultureInfo.InvariantCulture),
                                                      EndDate = DateTime.Parse("10/30/2010", CultureInfo.InvariantCulture),
                                                      Id = "1"
                                                },
                                            new ReportPeriod
                                                {
                                                      StartDate = DateTime.Parse("10/11/2010", CultureInfo.InvariantCulture),
                                                      EndDate = DateTime.Parse("03/03/2011", CultureInfo.InvariantCulture),
                                                      Id = "2"
                                                },
                                            new ReportPeriod
                                                {
                                                      StartDate = DateTime.Parse("03/04/2010", CultureInfo.InvariantCulture),
                                                      EndDate = DateTime.Parse("04/04/2011", CultureInfo.InvariantCulture),
                                                      Id = "3"
                                                }
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