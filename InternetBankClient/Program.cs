using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using log4net.Config;

namespace InternetBankClient
{
    public class CookieAwareWebClient : WebClient
    {
        private readonly CookieContainer _mContainer = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = _mContainer;
            }
            return request;
        }
    }

    public class SampleClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SampleClient));

        private static CookieCollection _cookies;

        private static PaymentCard _currentCard;

        private static int _maxDaysPeriod = 170;

        // callback used to validate the certificate in an SSL conversation
        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
        {
            bool result = false;
            if (cert.Subject.ToUpper().Contains("SBSIBANK"))
            {
                result = true;
            }

            return result;
        }

        private static void AddCommonHeadersToHttpRequest(HttpWebRequest request, string method = "GET")
        {
            // add cookies to request
            var cookieContainer = new CookieContainer();
            if (_cookies != null) cookieContainer.Add(_cookies);
            request.CookieContainer = cookieContainer;
            
            request.Method = method;
            request.ContentType = "application/x-www-form-urlencoded";
            request.PreAuthenticate = true;
            request.Host = "www.sbsibank.by";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:8.0) Gecko/20100101 Firefox/8.0";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Headers.Add("Accept-Language", "en-us,en;q=0.5");
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
        }

        public static void GetLogin()
        {
            // initialize login POST request
            var request = (HttpWebRequest)WebRequest.Create("https://www.sbsibank.by/login.asp");
            AddCommonHeadersToHttpRequest(request);
            var cookieContainer = new CookieContainer();
            request.CookieContainer = cookieContainer;
            request.Referer = "https://www.sbsibank.by/";

            // get response
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    foreach (Cookie cookie in response.Cookies)
                    {
                        Log.InfoFormat("Cookie name: {0} and value {1}", cookie.Name, cookie.Value);                        
                    }
                    _cookies = response.Cookies;
                    _cookies.Add(new Cookie("UN", "FQ529", "/", "www.sbsibank.by"));
                    _cookies.Add(new Cookie("C1", "checked", "/", "www.sbsibank.by"));
                    _cookies.Add(new Cookie("S1", "0175", "/", "www.sbsibank.by"));
                }
            }
        }

        public static bool Connect()
        {
            // initialize login POST request
            var request = (HttpWebRequest)WebRequest.Create("https://www.sbsibank.by/login.asp?mode=1");
            AddCommonHeadersToHttpRequest(request, "POST");
            request.Referer = "https://www.sbsibank.by/login.asp";

            // submit login form data
            const string postData = "S1=0175&T1=FQ529&C1=ON&T2=XUI4K&B1=%C4%E0%EB%E5%E5";
            Log.Info("Posting login form to " + "https://www.sbsibank.by/login.asp?mode=1" + " with content: " + postData);
            var encoding = new ASCIIEncoding();
            var data = encoding.GetBytes(postData);
            request.ContentLength = postData.Length;
            var dataStream = request.GetRequestStream();
            dataStream.Write(data, 0, data.Length);
            dataStream.Close();

            // get response
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                Log.InfoFormat("Status code on login data post is {0}", response.StatusCode);
                return response.StatusCode == HttpStatusCode.OK;
            }
        }

        public static IEnumerable<PaymentCard> GetCardList()
        {
            // initialize right.asp get request
            var request = (HttpWebRequest)WebRequest.Create("https://www.sbsibank.by/right.asp");
            AddCommonHeadersToHttpRequest(request);
            request.Referer = "https://www.sbsibank.by/home.asp";
            
            // get response
            Log.Info("Openning https://www.sbsibank.by/right.asp");
            // get response
            using (var response = (HttpWebResponse) request.GetResponse())
            {
                var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    return ParseHtmlHelper.ParseCardList(new StreamReader(responseStream, Encoding.GetEncoding(1251)));
                }
                return null;
            }
        }

        public static bool ChangeCurrentCard(PaymentCard card)
        {
            // initialize mbottom.asp get request
            var request = (HttpWebRequest)WebRequest.Create(
                String.Concat("https://www.sbsibank.by/mbottom.asp?crd_id=", card.Id));
            AddCommonHeadersToHttpRequest(request);
            request.Referer = "https://www.sbsibank.by/right.asp";

            // get response
            Log.InfoFormat("Change current card id to {0} at https://www.sbsibank.by/mbottom.asp?crd_id={0}", card.Id);
            // get response
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _currentCard = card;
                    return true;
                }
            }
            return false;
        }

        public static bool GetCurrentCardBalance(out string currency, out decimal balance)
        {
            // initialize balance.asp get request
            var request = (HttpWebRequest)WebRequest.Create("https://www.sbsibank.by/balance.asp");
            AddCommonHeadersToHttpRequest(request);
            request.Referer = "https://www.sbsibank.by/home.asp";

            // get response
            Log.Info("Open https://www.sbsibank.by/balance.asp");
            // get response
            balance = 0m;
            currency = "USD";
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    string strBalance;
                    ParseHtmlHelper.ParseBalance(new StreamReader(responseStream, Encoding.GetEncoding(1251)), out strBalance, out currency);
                    strBalance = String.Join(null, Regex.Split(strBalance, "[^\\d]"));
                    balance = Decimal.Parse(strBalance);
                    Log.InfoFormat("Balance is {0}", balance);
                    return true;
                }
            }
            return false;
        }

        public static bool GetCurrentCardHistory()
        {
            // initialize card_history.asp get request
            var request = (HttpWebRequest)WebRequest.Create("https://www.sbsibank.by/card_history.asp");
            AddCommonHeadersToHttpRequest(request);
            request.Referer = "https://www.sbsibank.by/right.asp";

            // get response
            Log.Info("Open https://www.sbsibank.by/card_history.asp");
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    string status;
                    DateTime registerDate;
                    DateTime cancelDate;
                    ParseHtmlHelper.ParseCardHistory(new StreamReader(responseStream, Encoding.GetEncoding(1251)), out status, out registerDate, out cancelDate);

                    _currentCard.Status = status;
                    _currentCard.RegisterDate = registerDate;
                    _currentCard.CancelDate = cancelDate;
                    Log.InfoFormat("Status is {0}", status);
                    Log.InfoFormat("Register Date is {0}", registerDate);
                    Log.InfoFormat("Cancel Date is {0}", cancelDate);
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<ReportRow> GetListOfUsedStatements(DateTime startDate)
        {
            if (startDate == DateTime.MinValue)
                startDate = _currentCard.RegisterDate;

            // initialize statementA.asp get request
            var request = (HttpWebRequest)WebRequest.Create("https://www.sbsibank.by/statementA.asp");
            AddCommonHeadersToHttpRequest(request);
            request.Referer = "https://www.sbsibank.by/left.asp";

            // get response
            Log.Info("Open https://www.sbsibank.by/statementA.asp");
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    var reports = ParseHtmlHelper.ParseStatementsList(new StreamReader(responseStream, Encoding.GetEncoding(1251)));
                    var endDate = DateTime.UtcNow.AddDays(-1);

                    var preparedRanges = new List<ReportRow>();
                    var reportsToCreate = new List<ReportRow>();
                    if (reports != null)
                    {
                        ReportRow range = null;
                        do
                        {
                            if (startDate > endDate) continue;

                            range = reports.Where(r => r.PeriodStartDate <= startDate && r.PeriodEndDate > startDate)
                                        .OrderByDescending(r => r.PeriodEndDate).FirstOrDefault() ??
                                    reports.Where(r => r.PeriodStartDate > startDate).OrderBy(r => r.PeriodStartDate)
                                        .FirstOrDefault();

                            if (range == null) continue;

                            range.IsCreated = true;
                            Log.Info(range.ToString());

                            if (range.PeriodStartDate > startDate)
                            {
                                var start = startDate;
                                do
                                {
                                    if (DaysBetween(range.PeriodStartDate, start) > _maxDaysPeriod)
                                    {
                                        var end = start.AddDays(_maxDaysPeriod);
                                        var createReport = new ReportRow
                                                               {
                                                                   PeriodStartDate = start,
                                                                   PeriodEndDate = end,
                                                                   IsCreated = false
                                                               };
                                        reportsToCreate.Add(createReport);
                                        Log.Info(createReport.ToString());

                                        start = end.AddDays(1);
                                    }
                                    else
                                    {
                                        var createReport = new ReportRow
                                        {
                                            PeriodStartDate = start,
                                            PeriodEndDate = range.PeriodStartDate.AddDays(-1),
                                            IsCreated = false
                                        };
                                        reportsToCreate.Add(createReport);
                                        Log.Info(createReport.ToString());
                                        start = range.PeriodStartDate.AddDays(1);
                                    }
                                    
                                } while (start < range.PeriodStartDate);
                            }
                            preparedRanges.Add(range);
                            startDate = range.PeriodEndDate.AddDays(1);
                        } while (range != null);
                    }

                    var start2 = startDate;
                    do
                    {
                        if (DaysBetween(endDate, start2) > _maxDaysPeriod)
                        {
                            var end = start2.AddDays(_maxDaysPeriod);
                            var createReport = new ReportRow
                            {
                                PeriodStartDate = start2,
                                PeriodEndDate = end,
                                IsCreated = false
                            };
                            reportsToCreate.Add(createReport);
                            Log.Info(createReport.ToString());

                            start2 = end.AddDays(1);
                        }
                        else
                        {
                            var createReport = new ReportRow
                            {
                                PeriodStartDate = start2,
                                PeriodEndDate = endDate,
                                IsCreated = false
                            };
                            reportsToCreate.Add(createReport);
                            Log.Info(createReport.ToString());
                            start2 = endDate;
                        }

                    } while (start2 < endDate);

                    return preparedRanges;
                }
            }
            return null;
        }

        public static int DaysBetween(DateTime d1, DateTime d2)
        {
            var span = d2.Subtract(d1);
            return Math.Abs((int)span.TotalDays);
        }

        public static void CreateReport(ReportRow range)
        {
            
        }

        public static bool GetReportData(string id)
        {
            var url = String.Format("https://www.sbsibank.by/show.asp?id={0}", id);
            // initialize card_history.asp get request
            var request = (HttpWebRequest)WebRequest.Create(url);
            AddCommonHeadersToHttpRequest(request);
            request.Referer = "https://www.sbsibank.by/statementA.asp";

            // get response
            Log.InfoFormat("Open {0}", url);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    ParseHtmlHelper.ParseReport(new StreamReader(responseStream, Encoding.GetEncoding(1251)));

                    return true;
                }
            }
            return false;
        }
    }

    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        static void Main()
        {
            XmlConfigurator.Configure();
            var sw = new Stopwatch();
            sw.Start();
            SampleClient.GetLogin();
            if (SampleClient.Connect())
            {
                var cards = SampleClient.GetCardList();
                foreach (var paymentCard in cards)
                {
                    if (SampleClient.ChangeCurrentCard(paymentCard))
                    {
                        // get currency and balance
                        decimal balance;
                        string currency;
                        SampleClient.GetCurrentCardBalance(out currency, out balance);
                        paymentCard.Currency = currency;
                        paymentCard.Balance = balance;

                        // get registered date, cancelation date and status
                        SampleClient.GetCurrentCardHistory();

                        // get date ranges for obtaining transactions
                        var ranges = SampleClient.GetListOfUsedStatements(DateTime.MinValue);

                        foreach (var range in ranges)
                        {
                            SampleClient.GetReportData(range.Id);
                        }
                    }
                }
            }
            sw.Stop();
            Log.InfoFormat("Time passed milliseconds: {0}", sw.ElapsedMilliseconds);
            Console.ReadLine();
        }
    }
}
