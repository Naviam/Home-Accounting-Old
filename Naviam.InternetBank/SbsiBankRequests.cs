using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Naviam.InternetBank.Entities;
using Naviam.InternetBank.Helpers;

namespace Naviam.InternetBank
{
    public class SbsiBankRequests
    {
        private CookieCollection _cookies;
        private readonly Uri _baseUri;

        public InetBankSettings Settings { get; private set; }

        public SbsiBankRequests(InetBankSettings settings)
        {
            Settings = settings;
            _baseUri = new Uri(Settings.BaseUrl);
        }

        private InetBankRequest GetRequestSettings(string category, string name, string method)
        {
            return Settings.Requests.FirstOrDefault(lr =>
                String.Compare(lr.Category, category, StringComparison.OrdinalIgnoreCase) == 0 &&
                String.Compare(lr.Name, name, StringComparison.OrdinalIgnoreCase) == 0 &&
                String.Compare(lr.Method, method, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private HttpWebRequest GetRequest(InetBankRequest request, bool followRedirect = true, string method = "GET")
        {
            return BankUtility.GetRequest(request, _cookies, _baseUri, Settings.RequestHeaders, followRedirect, method);
        }

        #region Login Methods
        /// <summary>
        /// Open the Login page to get Set-Cookies response header 
        /// </summary>
        /// <returns>Response code.
        ///     0: Success; 
        ///     1: Response status code is not OK;
        ///     2: Settings in XML has not been found for Login GET request.
        /// </returns>
        public int GetLoginPage()
        {
            // get bank settings for get login request
            var loginGetRequest = GetRequestSettings("login", "getloginpage", "GET");

            if (loginGetRequest != null)
            {
                var request = GetRequest(loginGetRequest);

                // get response
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        // set auth cookies if true
                        if (loginGetRequest.SetAuthCookies)
                            _cookies = response.Cookies;
                        #region Exapmple how to set own cookies if needed
                        //if (loginGetRequest.CookieCollection == null || !loginGetRequest.CookieCollection.Any())
                        //    return 3;
                        // parse custom cookies with named string formatter
                        //foreach (var cookie in loginGetRequest.CookieCollection)
                        //{
                        //    cookie.Value = cookie.Value.FormatWith(new { username, iBankId });
                        //    _cookies.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
                        //}
                        //_cookies = response.Cookies;
                        #endregion
                        return 0;
                    }
                    return 1;
                }
            }
            return 2;
        }

        /// <summary>
        /// Authenticate user in internet bank with POST request
        /// </summary>
        /// <param name="username">Internet Bank username.</param>
        /// <param name="password">Internet Bank password.</param>
        /// <param name="iBankId">Internet Bank ID.</param>
        /// <returns>
        ///     Response code:
        ///     0: Success;
        ///     1: Response status code is not 302;
        ///     2: XML settings have not been found for POST request;
        ///     3: Login Failed.
        /// </returns>
        public int Authenticate(string username, string password, string iBankId)
        {
            // get bank settings for get login request
            var loginPostRequest = GetRequestSettings("login", "authenticate", "POST");

            if (loginPostRequest != null)
            {
                var request = GetRequest(loginPostRequest, false, "POST");

                // submit login form data
                var postData = loginPostRequest.PostData.FormatWith(
                    new { username, password, iBankId });

                var encoding = new ASCIIEncoding();
                var data = encoding.GetBytes(postData);
                request.ContentLength = postData.Length;
                var dataStream = request.GetRequestStream();
                dataStream.Write(data, 0, data.Length);
                dataStream.Close();

                // get response
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.Found)
                    {
                        if (loginPostRequest.SetAuthCookies)
                        {
                            _cookies.Add(response.Cookies);
                        }
                        var isProtectedPage = response.Headers["Location"].Contains("home.asp");
                        return isProtectedPage ? 0 : 3;
                    }
                    return 1;
                }
            }
            return 2;
        }
        #endregion

        #region Card list Methods
        /// <summary>
        /// Get collection of user's payment cards
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PaymentCard> GetCardList()
        {
            var cardsGetRequest = GetRequestSettings("cards", "cardlist", "GET");

            if (cardsGetRequest != null)
            {
                var request = GetRequest(cardsGetRequest);

                // get response
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var responseStream = response.GetResponseStream();
                    return responseStream != null ?
                        SbsibankHtmlParser.ParseCardList(cardsGetRequest.Selector,
                        new StreamReader(responseStream, Encoding.GetEncoding(1251))) : new List<PaymentCard>();
                }
            }

            return new List<PaymentCard>();
        }

        public void UpdateCardInfo(ref PaymentCard card)
        {
            var result = ChangeCurrentCard(card.Id);
            if (!result) return;
            UpdateCardHistoryInfo(ref card);
            UpdateCardBalance(ref card);
        }

        /// <summary>
        /// Change current active card
        /// </summary>
        /// <param name="cardId"></param>
        public bool ChangeCurrentCard(string cardId)
        {
            var changeCardGetRequest = GetRequestSettings("cards", "changeActiveCard", "GET");

            if (changeCardGetRequest != null)
            {
                changeCardGetRequest.Url = changeCardGetRequest.Url.FormatWith(new { cardId });
                var request = GetRequest(changeCardGetRequest);

                // get response
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            return false;
        }

        public void UpdateCardHistoryInfo(ref PaymentCard paymentCard)
        {
            var historyCardGetRequest = GetRequestSettings("cards", "history", "GET");

            if (historyCardGetRequest == null) return;
            var request = GetRequest(historyCardGetRequest);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    SbsibankHtmlParser.ParseCardHistory(historyCardGetRequest.Selector,
                          new StreamReader(responseStream, Encoding.GetEncoding(1251)), ref paymentCard);
                }
            }
        }

        public void UpdateCardBalance(ref PaymentCard paymentCard)
        {
            var balanceCardGetRequest = GetRequestSettings("cards", "balance", "GET");

            if (balanceCardGetRequest == null) return;
            var request = GetRequest(balanceCardGetRequest);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    SbsibankHtmlParser.ParseBalance(
                        balanceCardGetRequest.Selector,
                        new StreamReader(responseStream, Encoding.GetEncoding(1251)), ref paymentCard);
                }
            }
        }
        #endregion

        #region Transactions Methods
        /// <summary>
        /// Get latest 20 transactions
        /// </summary>
        /// <returns>latest 20 transactions</returns>
        public IEnumerable<AccountTransaction> GetLatestCardTransactions()
        {
            var latestPostRequest = GetRequestSettings("cards", "latest", "POST");

            if (latestPostRequest != null)
            {
                var request = GetRequest(latestPostRequest, true, "POST");

                // submit login form data
                var postData = latestPostRequest.PostData;

                var encoding = new ASCIIEncoding();
                var data = encoding.GetBytes(postData);
                request.ContentLength = postData.Length;
                var dataStream = request.GetRequestStream();
                dataStream.Write(data, 0, data.Length);
                dataStream.Close();

                // get response
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var responseStream = response.GetResponseStream();
                    return responseStream != null ?
                        SbsibankHtmlParser.ParseLatestTransactions(latestPostRequest.Selector,
                        new StreamReader(responseStream, Encoding.GetEncoding(1251))) : new List<AccountTransaction>();
                }
            }
            return new List<AccountTransaction>();
        }

        public IEnumerable<ReportPeriod> GetListOfUsedStatements(DateTime startDate, DateTime cardRegisterDate)
        {
            if (startDate == DateTime.MinValue) startDate = cardRegisterDate;

            // get settings
            var statementsGetRequest = GetRequestSettings("cards", "statements", "GET");

            if (statementsGetRequest != null)
            {
                var request = GetRequest(statementsGetRequest);
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var responseStream = response.GetResponseStream();
                    if (responseStream != null)
                    {
                        // get all pregenerated reports
                        var reports = SbsibankHtmlParser.ParseStatementsList(
                            statementsGetRequest.Selector,
                            new StreamReader(responseStream, Encoding.GetEncoding(1251))).ToList();
                        // last day for sync is yesterday
                        var endDate = DateTime.UtcNow.AddDays(-1);

                        return BankUtility.GetReportPeriods(startDate, endDate, Settings.MaxDaysPeriod, reports);
                    }
                }
            }
            return null;
        }

        public Report CreateReport(DateTime startDate, DateTime endDate)
        {
            var createReportPostRequest = GetRequestSettings("cards", "createReport", "POST");

            if (createReportPostRequest != null)
            {
                var request = GetRequest(createReportPostRequest, true, "POST");

                // submit login form data
                var postData = createReportPostRequest.PostData;

                var encoding = new ASCIIEncoding();
                var data = encoding.GetBytes(postData);
                request.ContentLength = postData.Length;
                var dataStream = request.GetRequestStream();
                dataStream.Write(data, 0, data.Length);
                dataStream.Close();

                // get response
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var responseStream = response.GetResponseStream();
                    return responseStream != null ?
                        SbsibankHtmlParser.ParseReport(createReportPostRequest.Selector,
                        new StreamReader(responseStream, Encoding.GetEncoding(1251))) : null;
                }
            }
            return null;
        }
        #endregion
    }
}
