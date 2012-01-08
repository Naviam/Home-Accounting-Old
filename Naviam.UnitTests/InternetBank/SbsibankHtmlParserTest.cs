using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Naviam.InternetBank.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Naviam.InternetBank.Entities;
using System.Collections.Generic;

namespace Naviam.UnitTests.InternetBank
{
    /// <summary>
    ///This is a test class for SbsibankHtmlParserTest and is intended
    ///to contain all SbsibankHtmlParserTest Unit Tests
    ///</summary>
    [TestClass]
    public class SbsibankHtmlParserTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        public const string SbsibanksettingsXml = "SbsibankSettings.xml";
        public static InetBankSettings Settings;

        #region Additional test attributes
        [ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
            // load settings from xml
            var serializer = new XmlSerializer(typeof(InetBankSettings));
            if (!File.Exists(SbsibanksettingsXml)) return;
            using (var streamReader = File.OpenText(SbsibanksettingsXml))
            {
                Settings = serializer.Deserialize(streamReader) as InetBankSettings;
            }
        }

        private static StreamReader OpenHtmlFile(string fileName)
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, 
                String.Concat("HtmlSamples\\", fileName));
            return new StreamReader(
                File.Open(filePath, FileMode.Open), Encoding.GetEncoding(1251));
        }

        private static InetBankRequest GetRequestSettings(string category, string name, string method)
        {
            return Settings.Requests.FirstOrDefault(lr =>
                String.Compare(lr.Category, category, StringComparison.OrdinalIgnoreCase) == 0 &&
                String.Compare(lr.Name, name, StringComparison.OrdinalIgnoreCase) == 0 &&
                String.Compare(lr.Method, method, StringComparison.OrdinalIgnoreCase) == 0);
        }
        #endregion

        /// <summary>
        ///A test for ParseBalance
        ///</summary>
        [TestMethod]
        public void ParseBalanceTest()
        {
            // arrange
            var request = GetRequestSettings("cards", "balance", "GET");

            var content = OpenHtmlFile("balance.htm");

            var card = new PaymentCard();
            var cardExpected = new PaymentCard {Balance = 4690477, Currency = "BYR"};
            
            // act
            Debug.Assert(request != null, "request != null");
            SbsibankHtmlParser.ParseBalance(request.Selector, content, ref card);
            
            // assert
            Assert.AreEqual(cardExpected.Balance, card.Balance);
            Assert.AreEqual(cardExpected.Currency, card.Currency);
        }

        /// <summary>
        ///A test for ParseCardHistory
        ///</summary>
        [TestMethod]
        public void ParseCardHistoryTest()
        {
            // arrange
            var request = GetRequestSettings("cards", "history", "GET");

            var content = OpenHtmlFile("card_history.htm");

            var card = new PaymentCard();
            var cardExpected = new PaymentCard
                                   {
                                       RegisterDate = DateTime.Parse("11.02.2010 14:10:35"),
                                       Status = "АКТИВНА"
                                   };
            
            // act
            Debug.Assert(request != null, "request != null");
            SbsibankHtmlParser.ParseCardHistory(request.Selector, content, ref card);
            
            // assert
            Assert.AreEqual(cardExpected.RegisterDate, card.RegisterDate);
            Assert.AreEqual(cardExpected.CancelDate, card.CancelDate);
            Assert.AreEqual(cardExpected.Status, card.Status);
        }

        /// <summary>
        ///A test for ParseCardList
        ///</summary>
        [TestMethod]
        public void ParseCardListTest()
        {
            // arrange
            var request = GetRequestSettings("cards", "cardlist", "GET");

            var content = OpenHtmlFile("CardList.htm");

            IEnumerable<PaymentCard> expected = new List<PaymentCard>
                                                    {
                                                        new PaymentCard
                                                            {
                                                                Id = "1446491",
                                                                Name = "4.3855 Visa ZP BYR"
                                                            }
                                                    };

            // act
            Debug.Assert(request != null, "request != null");
            var actual = SbsibankHtmlParser.ParseCardList(request.Selector, content).ToList();
            Debug.Assert(actual != null, "actual != null");
            var actualFirst = actual.FirstOrDefault();

            // assert
            Assert.AreEqual(expected.Count(), actual.Count());
            Debug.Assert(actualFirst != null, "actualFirst != null");
            Assert.AreEqual(expected.First().Id, actualFirst.Id);
            Assert.AreEqual(expected.First().Name, actualFirst.Name);
        }

        /// <summary>
        ///A test for ParseLatestTransactions
        ///</summary>
        [TestMethod]
        public void ParseLatestTransactionsTest()
        {
            // arrange
            var request = GetRequestSettings("transactions", "latest", "POST");

            var content = OpenHtmlFile("latesttransactions2.htm");

            var expected = PrepareExpectedLatestTransactions().ToList();

            // act
            Debug.Assert(request != null, "request != null");
            var actual = SbsibankHtmlParser.ParseLatestTransactions(request.Selector, content).ToList();
            
            // assert
            Assert.AreEqual(expected.Count(), actual.Count());
            var count = expected.Count();
            for (var i = 0; i < count; i++)
            {
                var exp = expected[i];
                var act = actual[i];
                Assert.AreEqual(exp.OperationDate, act.OperationDate, String.Format("Operation Date, index: {0}", i));
                Assert.AreEqual(exp.Status, act.Status, String.Format("Status, index: {0}", i));
                Assert.AreEqual(exp.OperationAmount, act.OperationAmount, String.Format("Operation Amount, index: {0}", i));
                Assert.AreEqual(exp.Currency, act.Currency, String.Format("Currency, index: {0}", i));
                Assert.AreEqual(exp.OperationDescription, act.OperationDescription, String.Format("Operation Description, index: {0}", i));
            }
        }

        /// <summary>
        ///A test for ParseReport
        ///</summary>
        [TestMethod]
        public void ParseReportTest()
        {
            // arrange
            var request = GetRequestSettings("transactions", "getreport", "GET");
            var content = OpenHtmlFile("Report.htm");
            var expected = PrepareExpectedReport();

            // act
            Debug.Assert(request != null, "request != null");
            var actual = SbsibankHtmlParser.ParseReport(request.Selector, content);
            var expectedTransactions = expected.Transactions.ToList();
            var actualTransactions = actual.Transactions.ToList();

            // assert
            Assert.AreEqual(expected.CardNumber, actual.CardNumber, "Card Number");
            Assert.AreEqual(expected.Currency, actual.Currency, "Card Number");
            Assert.AreEqual(expected.StartBalance, actual.StartBalance, "Start Balance");
            Assert.AreEqual(expected.StateOnDate.ToString(), actual.StateOnDate.ToString(), "State on date");
            Assert.AreEqual(expected.ReportPeriod.ToString(), actual.ReportPeriod.ToString(), "Report Period");
            Assert.AreEqual(expectedTransactions.Count(), actualTransactions.Count(), "Count of transactions");
            var count = expectedTransactions.Count();
            for (var i = 0; i < count; i++)
            {
                var exp = expectedTransactions[i];
                var act = actualTransactions[i];
                Assert.AreEqual(exp.OperationDate, act.OperationDate, String.Format("Operation Date, index: {0}", i));
                Assert.AreEqual(exp.Status, act.Status, String.Format("Status, index: {0}", i));
                Assert.AreEqual(exp.TransactionAmount, act.TransactionAmount, String.Format("Transaction Amount, index: {0}", i));
                Assert.AreEqual(exp.Currency, act.Currency, String.Format("Currency, index: {0}", i));
                Assert.AreEqual(exp.OperationDescription, act.OperationDescription, String.Format("Operation Description, index: {0}", i));
            }
        }

        /// <summary>
        ///A test for ParseStatementsList
        ///</summary>
        [TestMethod]
        public void ParseStatementsListTest()
        {
            // arrange
            var request = Settings.Requests
                .FirstOrDefault(c => String.Compare(c.Name, "statements", StringComparison.OrdinalIgnoreCase) == 0);

            var content = OpenHtmlFile("Statements.htm");

            var expected = new List<ReportPeriod>
                            {
                                new ReportPeriod
                                    {
                                        StartDate = DateTime.Parse("2011-01-01"),
                                        EndDate = DateTime.Parse("2011-05-01"),
                                        CreatedDate = DateTime.Parse("2011-12-01 04:01:13"),
                                        Id = "116167"
                                    },
                                new ReportPeriod
                                    {
                                        StartDate = DateTime.Parse("2010-02-11"),
                                        EndDate = DateTime.Parse("2010-05-01"),
                                        CreatedDate = DateTime.Parse("2011-12-07 16:53:42"),
                                        Id = "120946"
                                    }
                            };
            
            // act
            Debug.Assert(request != null, "request != null");
            var actual = SbsibankHtmlParser.ParseStatementsList(request.Selector, content).ToList();
            
            // assert
            var count = expected.Count();
            for (var i = 0; i < count; i++)
            {
                var exp = expected[i];
                var act = actual[i];
                Assert.AreEqual(exp.StartDate, act.StartDate, String.Format("Start Date, index: {0}", i));
                Assert.AreEqual(exp.EndDate, act.EndDate, String.Format("End Date, index: {0}", i));
                Assert.AreEqual(exp.CreatedDate, act.CreatedDate, String.Format("Created Date, index: {0}", i));
                Assert.AreEqual(exp.Id, act.Id, String.Format("Id, index: {0}", i));
            }
        }

        private static Report PrepareExpectedReport()
        {
            var report = new Report
            {
                ReportPeriod = new ReportPeriod
                                   {
                                       StartDate = DateTime.Parse("01/01/2011"),
                                       EndDate = DateTime.Parse("01/05/2011")
                                   },
                CardNumber = "4XXX-XXXX-XXXX-3855",
                StartBalance = 496667,
                Currency = "BYR",
                StateOnDate = new StateOnDate
                                  {
                                      Date = DateTime.Parse("01/12/2011"),
                                      Available = 178017,
                                      BlockedAmount = 0,
                                      CreditLimit = 0
                                  },
                Transactions = new List<AccountTransaction>
                {
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("31/12/10"),
                            Status = "F",
                            OperationAmount = 480,
                            Currency = "BYR",
                            OperationDescription = "Service payment to card   RBS Balance loader",
                            TransactionDate = DateTime.Parse("03/01/11"),
                            Commission = 0,
                            TransactionAmount = 480
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("31/12/10"),
                            Status = "F",
                            OperationAmount = -17940,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK APTEKA N 5 \"BELFARM-CENTR",
                            TransactionDate = DateTime.Parse("05/01/11"),
                            Commission = 0,
                            TransactionAmount = -17940
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("31/12/10"),
                            Status = "F",
                            OperationAmount = -83890,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK SHOP N 1 \"KLASSICHESK.VIN",
                            TransactionDate = DateTime.Parse("05/01/11"),
                            Commission = 0,
                            TransactionAmount = -83890
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("31/12/10"),
                            Status = "F",
                            OperationAmount = -147290,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK UNIVERSAM \"SOSEDI\"",
                            TransactionDate = DateTime.Parse("05/01/11"),
                            Commission = 0,
                            TransactionAmount = -147290
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("02/01/11"),
                            Status = "F",
                            OperationAmount = -177800,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK UNIVERSAM \"SOSEDI\"",
                            TransactionDate = DateTime.Parse("05/01/11"),
                            Commission = 0,
                            TransactionAmount = -177800
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("05/01/11"),
                            Status = "F",
                            OperationAmount = 527870,
                            Currency = "BYR",
                            OperationDescription = "Service payment to card   RBS Balance loader",
                            TransactionDate = DateTime.Parse("06/01/11"),
                            Commission = 0,
                            TransactionAmount = 527870
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("08/01/11"),
                            Status = "F",
                            OperationAmount = -400000,
                            Currency = "BYR",
                            OperationDescription = "ATM BLR MINSK ATMMMB HO12 TPP KOLASA",
                            TransactionDate = DateTime.Parse("10/01/11"),
                            Commission = 0,
                            TransactionAmount = -400000
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("06/01/11"),
                            Status = "F",
                            OperationAmount = -16370,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK MIDIMARKET \"KOLBASYR\"",
                            TransactionDate = DateTime.Parse("12/01/11"),
                            Commission = 0,
                            TransactionAmount = -16370
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("06/01/11"),
                            Status = "F",
                            OperationAmount = -116440,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK UNIVERSAM \"SOSEDI\"",
                            TransactionDate = DateTime.Parse("12/01/11"),
                            Commission = 0,
                            TransactionAmount = -116440
                        },
                    new AccountTransaction //10
                        {
                            OperationDate = DateTime.Parse("12/01/11"),
                            Status = "F",
                            OperationAmount = -30000,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK VELCOM I- BANK",
                            TransactionDate = DateTime.Parse("13/01/11"),
                            Commission = 0,
                            TransactionAmount = -30000
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("11/01/11"),
                            Status = "F",
                            OperationAmount = -19990,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK MIDIMARKET \"KOLBASYR\"",
                            TransactionDate = DateTime.Parse("14/01/11"),
                            Commission = 0,
                            TransactionAmount = -19990
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("14/01/11"),
                            Status = "F",
                            OperationAmount = -13780,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK MIDIMARKET \"KOLBASYR\"",
                            TransactionDate = DateTime.Parse("19/01/11"),
                            Commission = 0,
                            TransactionAmount = -13780
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("20/01/11"),
                            Status = "F",
                            OperationAmount = 563620,
                            Currency = "BYR",
                            OperationDescription = "Service payment to card   RBS Balance loader",
                            TransactionDate = DateTime.Parse("21/01/11"),
                            Commission = 0,
                            TransactionAmount = 563620
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("22/01/11"),
                            Status = "F",
                            OperationAmount = -17930,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK UNIVERSAM \"SOSEDI\"",
                            TransactionDate = DateTime.Parse("26/01/11"),
                            Commission = 0,
                            TransactionAmount = -17930
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("31/01/11"),
                            Status = "F",
                            OperationAmount = 500,
                            Currency = "BYR",
                            OperationDescription = "Service payment to card   RBS Balance loader",
                            TransactionDate = DateTime.Parse("01/02/11"),
                            Commission = 0,
                            TransactionAmount = 500
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("29/01/11"),
                            Status = "F",
                            OperationAmount = -96000,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK SHOP \"MOTHERCARE\"",
                            TransactionDate = DateTime.Parse("02/02/11"),
                            Commission = 0,
                            TransactionAmount = -96000
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("02/02/11"),
                            Status = "F",
                            OperationAmount = -440000,
                            Currency = "BYR",
                            OperationDescription = "ATM BLR MINSK ATMMMB HO12 TPP KOLASA",
                            TransactionDate = DateTime.Parse("02/02/11"),
                            Commission = 0,
                            TransactionAmount = -440000
                        },
                    new AccountTransaction //18
                        {
                            OperationDate = DateTime.Parse("04/02/11"),
                            Status = "F",
                            OperationAmount = 518880,
                            Currency = "BYR",
                            OperationDescription = "Service payment to card   RBS Balance loader",
                            TransactionDate = DateTime.Parse("07/02/11"),
                            Commission = 0,
                            TransactionAmount = 518880
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("14/02/11"),
                            Status = "F",
                            OperationAmount = -500000,
                            Currency = "BYR",
                            OperationDescription = "ATM BLR MINSK ATMMMB HO12 TPP KOLASA",
                            TransactionDate = DateTime.Parse("14/02/11"),
                            Commission = 0,
                            TransactionAmount = -500000
                        },
                    new AccountTransaction //20
                        {
                            OperationDate = DateTime.Parse("18/02/11"),
                            Status = "F",
                            OperationAmount = 563620,
                            Currency = "BYR",
                            OperationDescription = "Service payment to card   RBS Balance loader",
                            TransactionDate = DateTime.Parse("21/02/11"),
                            Commission = 0,
                            TransactionAmount = 563620
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("21/02/11"),
                            Status = "F",
                            OperationAmount = -400000,
                            Currency = "BYR",
                            OperationDescription = "ATM BLR MINSK ATMMMB HO12 TPP KOLASA",
                            TransactionDate = DateTime.Parse("21/02/11"),
                            Commission = 0,
                            TransactionAmount = -400000
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("19/02/11"),
                            Status = "F",
                            OperationAmount = -32270,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK SHOP \"KOMAROVSKIY\"",
                            TransactionDate = DateTime.Parse("23/02/11"),
                            Commission = 0,
                            TransactionAmount = -32270
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("19/02/11"),
                            Status = "F",
                            OperationAmount = -11970,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK SHOP \"KOMAROVSKIY\"",
                            TransactionDate = DateTime.Parse("23/02/11"),
                            Commission = 0,
                            TransactionAmount = -11970
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("19/02/11"),
                            Status = "F",
                            OperationAmount = -47540,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK CUM \"MINSK\"",
                            TransactionDate = DateTime.Parse("23/02/11"),
                            Commission = 0,
                            TransactionAmount = -47540
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("21/02/11"),
                            Status = "F",
                            OperationAmount = -15080,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK PT SHOP KNIJNIY UP JIZN'",
                            TransactionDate = DateTime.Parse("24/02/11"),
                            Commission = 0,
                            TransactionAmount = -15080
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("21/02/11"),
                            Status = "F",
                            OperationAmount = -6010,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK PT SHOP KNIJNIY UP JIZN'",
                            TransactionDate = DateTime.Parse("24/02/11"),
                            Commission = 0,
                            TransactionAmount = -6010
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("04/03/11"),
                            Status = "F",
                            OperationAmount = 370,
                            Currency = "BYR",
                            OperationDescription = "Service payment to card   RBS Balance loader",
                            TransactionDate = DateTime.Parse("04/03/11"),
                            Commission = 0,
                            TransactionAmount = 370
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("04/03/11"),
                            Status = "F",
                            OperationAmount = 518870,
                            Currency = "BYR",
                            OperationDescription = "Service payment to card   RBS Balance loader",
                            TransactionDate = DateTime.Parse("09/03/11"),
                            Commission = 0,
                            TransactionAmount = 518870
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("05/03/11"),
                            Status = "F",
                            OperationAmount = -500000,
                            Currency = "BYR",
                            OperationDescription = "ATM BLR MINSK ATMMMB HO12 TPP KOLASA",
                            TransactionDate = DateTime.Parse("09/03/11"),
                            Commission = 0,
                            TransactionAmount = -500000
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("10/03/11"),
                            Status = "F",
                            OperationAmount = 918270,
                            Currency = "BYR",
                            OperationDescription = "Service payment to card   RBS Balance loader",
                            TransactionDate = DateTime.Parse("11/03/11"),
                            Commission = 0,
                            TransactionAmount = 918270
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("14/03/11"),
                            Status = "F",
                            OperationAmount = 0,
                            Currency = "BYR",
                            OperationDescription = "Balance Enquire Visa BLR MINSK  ATM180 VC \"AQUABEL\"",
                            TransactionDate = DateTime.Parse("14/03/11"),
                            Commission = -3500,
                            TransactionAmount = -3500
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("12/03/11"),
                            Status = "F",
                            OperationAmount = -950000,
                            Currency = "BYR",
                            OperationDescription = "ATM BLR MINSK 00000000180/ATM180 VC \"AQ",
                            TransactionDate = DateTime.Parse("14/03/11"),
                            Commission = -19000,
                            TransactionAmount = -969000
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("21/03/11"),
                            Status = "F",
                            OperationAmount = -40000,
                            Currency = "BYR",
                            OperationDescription = "ATM BLR MINSK ATMMMB HO04 RIGA",
                            TransactionDate = DateTime.Parse("21/03/11"),
                            Commission = 0,
                            TransactionAmount = -40000
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("01/04/11"),
                            Status = "F",
                            OperationAmount = 310,
                            Currency = "BYR",
                            OperationDescription = "Service payment to card   RBS Balance loader",
                            TransactionDate = DateTime.Parse("01/04/11"),
                            Commission = 0,
                            TransactionAmount = 310
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("05/04/11"),
                            Status = "F",
                            OperationAmount = 277250,
                            Currency = "BYR",
                            OperationDescription = "Service payment to card   RBS Balance loader",
                            TransactionDate = DateTime.Parse("06/04/11"),
                            Commission = 0,
                            TransactionAmount = 277250
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("05/04/11"),
                            Status = "F",
                            OperationAmount = -200000,
                            Currency = "BYR",
                            OperationDescription = "ATM BLR MINSK ATMMMB HO20 BOGDANOVICHA",
                            TransactionDate = DateTime.Parse("06/04/11"),
                            Commission = 0,
                            TransactionAmount = -200000
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("09/04/11"),
                            Status = "F",
                            OperationAmount = -23000,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK VELCOM I- BANK",
                            TransactionDate = DateTime.Parse("11/04/11"),
                            Commission = 0,
                            TransactionAmount = -23000
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("21/04/11"),
                            Status = "F",
                            OperationAmount = 573340,
                            Currency = "BYR",
                            OperationDescription = "Service payment to card   RBS Balance loader",
                            TransactionDate = DateTime.Parse("22/04/11"),
                            Commission = 0,
                            TransactionAmount = 573340
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("22/04/11"),
                            Status = "F",
                            OperationAmount = -500000,
                            Currency = "BYR",
                            OperationDescription = "ATM BLR MINSK ATMMMB HO04 RIGA",
                            TransactionDate = DateTime.Parse("25/04/11"),
                            Commission = 0,
                            TransactionAmount = -500000
                        },
                    new AccountTransaction
                        {
                            OperationDate = DateTime.Parse("22/04/11"),
                            Status = "F",
                            OperationAmount = -104010,
                            Currency = "BYR",
                            OperationDescription = "Retail BLR MINSK UNIVERSAM \"SOSEDI\"",
                            TransactionDate = DateTime.Parse("27/04/11"),
                            Commission = 0,
                            TransactionAmount = -104010
                        }
                }
            };
            return report;
        }

        private static IEnumerable<AccountTransaction> PrepareExpectedLatestTransactions()
        {
            return new List<AccountTransaction>
                    {
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("06.01.2012"),
                                Status = "A",
                                OperationAmount = -100000,
                                Currency = "BYR",
                                OperationDescription = "ATMMMB HO04 RIGA"
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("06.01.2012"),
                                Status = "A",
                                OperationAmount = -200000,
                                Currency = "BYR",
                                OperationDescription = "ATMMMB HO04 RIGA"
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("06.01.2012"),
                                Status = "A",
                                OperationAmount = -340000,
                                Currency = "BYR",
                                OperationDescription = "PT SHOP \"MIAMI\""
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("05.01.2012"),
                                Status = "F",
                                OperationAmount = 3994450,
                                Currency = "BYR",
                                OperationDescription = "RBS Balance loader"
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("31.12.2011"),
                                Status = "F",
                                OperationAmount = -8300,
                                Currency = "BYR",
                                OperationDescription = "RBS Balance loader"
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("17.12.2011"),
                                Status = "F",
                                OperationAmount = -400000,
                                Currency = "BYR",
                                OperationDescription = "ATMBVB HO36 RUBIN PLAZA"
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("17.12.2011"),
                                Status = "F",
                                OperationAmount = -500000,
                                Currency = "BYR",
                                OperationDescription = "ATMBVB HO36 RUBIN PLAZA"
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("17.12.2011"),
                                Status = "F",
                                OperationAmount = -40000,
                                Currency = "BYR",
                                OperationDescription = "MTS I-BANK"
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("17.12.2011"),
                                Status = "F",
                                OperationAmount = -1445200,
                                Currency = "BYR",
                                OperationDescription = "KRAVT SHOP"
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("17.12.2011"),
                                Status = "F",
                                OperationAmount = -111620,
                                Currency = "BYR",
                                OperationDescription = "SHOP \"FANTASTIK\""
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("16.12.2011"),
                                Status = "F",
                                OperationAmount = -110800,
                                Currency = "BYR",
                                OperationDescription = "MIDIMARKET \"KOLBASYR\""
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("16.12.2011"),
                                Status = "F",
                                OperationAmount = -202390,
                                Currency = "BYR",
                                OperationDescription = "UNIVERSAM \"SOSEDI\""
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("15.12.2011"),
                                Status = "F",
                                OperationAmount = -53570,
                                Currency = "BYR",
                                OperationDescription = "MIDIMARKET \"KOLBASYR\""
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("15.12.2011"),
                                Status = "F",
                                OperationAmount = -240110,
                                Currency = "BYR",
                                OperationDescription = "MIDIMARKET \"KOLBASYR\""
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("15.12.2011"),
                                Status = "F",
                                OperationAmount = -24520,
                                Currency = "BYR",
                                OperationDescription = "MIDIMARKET \"KOLBASYR\""
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("14.12.2011"),
                                Status = "F",
                                OperationAmount = -30000,
                                Currency = "BYR",
                                OperationDescription = "VELCOM I-BANK"
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("14.12.2011"),
                                Status = "F",
                                OperationAmount = -50000,
                                Currency = "BYR",
                                OperationDescription = "MTS I-BANK"
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("14.12.2011"),
                                Status = "F",
                                OperationAmount = -100000,
                                Currency = "BYR",
                                OperationDescription = "VELCOM I-BANK"
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("14.12.2011"),
                                Status = "F",
                                OperationAmount = -82560,
                                Currency = "BYR",
                                OperationDescription = "MIDIMARKET \"KOLBASYR\""
                        },
                        new AccountTransaction
                        {
                                OperationDate = DateTime.Parse("14.12.2011"),
                                Status = "F",
                                OperationAmount = -37370,
                                Currency = "BYR",
                                OperationDescription = "MIDIMARKET \"KOLBASYR\""
                        }
                    };
        }
    }
}
