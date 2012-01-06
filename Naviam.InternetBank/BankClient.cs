using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Naviam.InternetBank
{
    public class BankClient : IDisposable
    {
        private CookieCollection _cookies;
        private readonly Uri _baseUri;
        private const string BanksXmlFileName = "InternetBanks.xml";

        /// <summary>
        /// Current Internet bank 
        /// </summary>
        public InetBank InetBank { get; private set; }
        
        /// <summary>
        /// Internet Bank settings (rules, custom text) that helps to export and parse user's transactions
        /// </summary>
        public InetBankSettings Settings { get; private set; }

        /// <summary>
        /// Initialize the internet bank client with specific settings of a given bank.
        /// </summary>
        /// <param name="bankId">Internal Bank Id that is used to get internet bank settings</param>
        public BankClient(int bankId)
        {
            // deserialize xml bank settings to object 
            // and read internet bank settings for specified bank id 
            var banks = LoadBanks(BanksXmlFileName);
            InetBank = (from b in banks
                       where b.NaviamId == bankId.ToString(CultureInfo.InvariantCulture)
                       select b).FirstOrDefault();

            if (InetBank != null) Settings = LoadSettings(InetBank.BankSettings);
            _baseUri = new Uri(Settings.BaseUrl);
        }

        /// <summary>
        /// Login to the internet bank web site with provided inet bank credentials
        /// </summary>
        /// <param name="userName">IBank Username</param>
        /// <param name="password">IBank Password</param>
        /// <returns>Login response object</returns>
        public LoginResponse Login(string userName, string password)
        {
            var responseCode = GetLoginPage(userName, InetBank.BankId);
            if (responseCode == 0)
            {
                responseCode = Authenticate(userName, password, InetBank.BankId);
            }
            return new LoginResponse
                       {
                           ErrorCode = responseCode,
                           IsAuthenticated = responseCode == 0
                       };
        }

        /// <summary>
        /// Obtain the list of user's payment cards from internet bank web site.
        /// </summary>
        public IEnumerable<PaymentCard> GetPaymentCards()
        {
            var cardList = GetCardList();
            var resultCardList = new List<PaymentCard>();

            foreach (var paymentCard in cardList)
            {
                var card = paymentCard;
                if (paymentCard != null)
                {
                    UpdateCardInfo(ref card);
                    resultCardList.Add(card);
                }
            }
            return resultCardList;
        }

        /// <summary>
        /// Obtain the list of transactions for payment card starting from specified date.
        /// </summary>
        /// <param name="cardId">Card Id to get transactions for</param>
        /// <param name="startDate">Start date to obtain transactions from</param>
        public IEnumerable<AccountTransaction> GetTransactions(string cardId, DateTime startDate)
        {
            return new List<AccountTransaction>();
        }

        /// <summary>
        /// Logout from internet bank
        /// </summary>
        /// <param name="cleanAuthCookies">Indicates whether it is required to clean internet bank cookies on logout</param>
        public void Logout(bool cleanAuthCookies)
        {
            
        }

        public void Dispose()
        {
            Logout(true);
        }

        #region PRIVATE METHODS

        #region Login Methods
        /// <summary>
        /// Open the Login page to get Set-Cookies response header 
        /// </summary>
        /// <returns>Response code.
        ///     0: Success; 
        ///     1: Response status code is not OK;
        ///     2: Settings in XML has not been found for Login GET request;
        ///     3: Cookie collection in the XML settings was not found.
        /// </returns>
        private int GetLoginPage(string username, string iBankId)
        {
            Cookie setCookie = null;
            // get bank settings for get login request
            var loginGetRequest = Settings.LoginRequests
                .FirstOrDefault(lr => String.Compare(lr.Method, "GET", StringComparison.OrdinalIgnoreCase) == 0);

            if (loginGetRequest != null)
            {
                var request = GetRequest(loginGetRequest.Url, loginGetRequest.Referer);

                // get response
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        if (loginGetRequest.SetAuthCookies)
                        {
                            _cookies = response.Cookies;
                        }
                        if (loginGetRequest.CookieCollection == null || !loginGetRequest.CookieCollection.Any())
                            return 3;
                        // parse custom cookies with named string formatter
                        foreach (var cookie in loginGetRequest.CookieCollection)
                        {
                            cookie.Value = cookie.Value.FormatWith(new { username, iBankId });
                            _cookies.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
                        }
                        //_cookies = response.Cookies;
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
        ///     1: Response status code is not OK;
        ///     2: XML settings have not been found for POST request;
        ///     4: Login Failed.
        /// </returns>
        public int Authenticate(string username, string password, string iBankId)
        {
            // get bank settings for get login request
            var loginPostRequest = Settings.LoginRequests
                .FirstOrDefault(lr => String.Compare(lr.Method, "POST", StringComparison.OrdinalIgnoreCase) == 0);

            if (loginPostRequest != null)
            {
                var request = GetRequest(loginPostRequest.Url, loginPostRequest.Referer, false, "POST");

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
                        var isProtectedPage = response.ResponseUri.AbsoluteUri.Contains("home.asp");
                        return isProtectedPage ? 0 : 4;
                    }
                    return 1;
                }
            }
            return 2;
        } 
        #endregion

        /// <summary>
        /// Get collection of user's payment cards
        /// </summary>
        /// <returns></returns>
        private IEnumerable<PaymentCard> GetCardList()
        {
            var cardsGetRequest = Settings.CardListRequests
                .FirstOrDefault(lr => String.Compare(lr.Name, "cardlist", StringComparison.OrdinalIgnoreCase) == 0);

            if (cardsGetRequest != null)
            {
                var request = GetRequest(cardsGetRequest.Url, cardsGetRequest.Referer);

                // get response
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var responseStream = response.GetResponseStream();
                    return responseStream != null ?
                        ParseHtmlHelper.ParseCardList(cardsGetRequest.Selector,
                        new StreamReader(responseStream, Encoding.GetEncoding(1251))) : new List<PaymentCard>();
                }
            }

            return new List<PaymentCard>();
        }

        private void UpdateCardInfo(ref PaymentCard card)
        {
            ChangeCurrentCard(card.Id);
            UpdateCardHistoryInfo(ref card);
            UpdateCardBalance(ref card);
        }

        /// <summary>
        /// Change current active card
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        private bool ChangeCurrentCard(string cardId)
        {
            var changeCardGetRequest = Settings.CardListRequests
                .FirstOrDefault(card => String.Compare(card.Name, "changeActiveCard", StringComparison.OrdinalIgnoreCase) == 0);

            if (changeCardGetRequest != null)
            {
                var url = changeCardGetRequest.Url.FormatWith(new {cardId});
                var request = GetRequest(url, changeCardGetRequest.Referer);

                // get response
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void UpdateCardHistoryInfo(ref PaymentCard paymentCard)
        {
            var historyCardGetRequest = Settings.CardListRequests
                .FirstOrDefault(card => String.Compare(card.Name, "history", StringComparison.OrdinalIgnoreCase) == 0);

            if (historyCardGetRequest != null)
            {
                var request = GetRequest(historyCardGetRequest.Url, historyCardGetRequest.Referer);
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var responseStream = response.GetResponseStream();
                    if (responseStream != null)
                    {
                        ParseHtmlHelper.ParseCardHistory(historyCardGetRequest.Selector,
                            new StreamReader(responseStream, Encoding.GetEncoding(1251)), ref paymentCard);
                    }
                }
            }
        }

        private void UpdateCardBalance(ref PaymentCard paymentCard)
        {
            var balanceCardGetRequest = Settings.CardListRequests
                .FirstOrDefault(card => String.Compare(card.Name, "balance", StringComparison.OrdinalIgnoreCase) == 0);

            if (balanceCardGetRequest != null)
            {
                var request = GetRequest(balanceCardGetRequest.Url, balanceCardGetRequest.Referer);
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var responseStream = response.GetResponseStream();
                    if (responseStream != null)
                    {
                        ParseHtmlHelper.ParseBalance(
                            balanceCardGetRequest.Selector, 
                            new StreamReader(responseStream, Encoding.GetEncoding(1251)), ref paymentCard);
                    }
                }
            }
        }

        #region Common Methods
        private HttpWebRequest GetRequest(string url, string referer, bool followRedirect = true, string method = "GET")
        {
            var request = (HttpWebRequest)WebRequest.Create(GetAbsoluteUri(url));
            // allows for validation of SSL conversations
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            AddCommonHeadersToHttpRequest(request, referer, method, followRedirect);
            return request;
        }

        // callback used to validate the certificate in an SSL conversation
        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return cert.Subject.ToUpper().Contains("SBSIBANK");
        }

        private void AddCommonHeadersToHttpRequest(
            HttpWebRequest request, string referer, string method, bool followRedirect = true)
        {
            // add cookies to request
            var cookieContainer = new CookieContainer();
            if (_cookies != null) cookieContainer.Add(_cookies);
            request.CookieContainer = cookieContainer;

            request.Method = method;
            request.AllowAutoRedirect = followRedirect;
            request.KeepAlive = true;
            request.ContentType = Settings.RequestHeaders.ContentType;
            request.PreAuthenticate = Settings.RequestHeaders.PreAuthenticate;
            request.Host = Settings.RequestHeaders.Host;
            request.UserAgent = Settings.RequestHeaders.UserAgent;
            request.Accept = Settings.RequestHeaders.Accept;
            request.Headers.Add("Accept-Language", Settings.RequestHeaders.AcceptLanguage);
            request.Headers.Add("Accept-Encoding", Settings.RequestHeaders.AcceptEncoding);
            request.Headers.Add("Accept-Charset", Settings.RequestHeaders.AcceptCharset);

            Uri uriResult;
            if (Uri.TryCreate(_baseUri, referer, out uriResult))
                request.Referer = uriResult.AbsoluteUri;
        }

        private string GetAbsoluteUri(string relativeUri)
        {
            Uri resultUri;
            return Uri.TryCreate(_baseUri, relativeUri, out resultUri) ? resultUri.AbsoluteUri : String.Empty;
        }

        private static InetBankSettings LoadSettings(string sbsibanksettingsXml)
        {
            var serializer = new XmlSerializer(typeof(InetBankSettings));
            if (File.Exists(sbsibanksettingsXml))
            {
                using (var streamReader = File.OpenText(sbsibanksettingsXml))
                {
                    return serializer.Deserialize(streamReader) as InetBankSettings;
                }
            }
            return null;
        }

        private static IEnumerable<InetBank> LoadBanks(string fileName)
        {
            var serializer = new XmlSerializer(typeof(InternetBanks));
            if (File.Exists(fileName))
            {
                using (var streamReader = File.OpenText(fileName))
                {
                    var internetBanks = serializer.Deserialize(streamReader) as InternetBanks;
                    if (internetBanks != null)
                        return internetBanks.Banks;
                }
            }
            return null;
        }
        #endregion

        #endregion
    }

    public class LoginResponse
    {
        public bool IsAuthenticated { get; set; }
        public int ErrorCode { get; set; }
    }

    public class InetBankCookie
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "path")]
        public string Path { get; set; }
        [XmlAttribute(AttributeName = "domain")]
        public string Domain { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "internetBankSettings")]
    public class InetBankSettings
    {
        [XmlElement(ElementName = "baseUrl", Type = typeof(string))]
        public string BaseUrl { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "maxDaysPeriod", Type = typeof(int))]
        public int MaxDaysPeriod { get; set; }
        [XmlElement(ElementName = "commonHeaders", Type = typeof(RequestHeaders))]
        public RequestHeaders RequestHeaders { get; set; }
        [XmlArrayItem(ElementName = "request", Type = typeof(LoginRequest))]
        [XmlArray(ElementName = "login")]
        public LoginRequest[] LoginRequests { get; set; }
        [XmlArrayItem(ElementName = "request", Type = typeof(InetBankRequest))]
        [XmlArray(ElementName = "cardList")]
        public InetBankRequest[] CardListRequests { get; set; }
    }

    public class InetBankRequest
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }
        [XmlElement(ElementName = "url", Type = typeof(string))]
        public string Url { get; set; }
        [XmlElement(ElementName = "referer", Type = typeof(string))]
        public string Referer { get; set; }
        [XmlElement(ElementName = "selector", Type = typeof(string))]
        public string Selector { get; set; }
    }

    public class LoginRequest : InetBankRequest
    {
        [XmlElement(ElementName = "setAuthCookies", Type = typeof(bool))]
        public bool SetAuthCookies { get; set; }
        [XmlArrayItem(ElementName = "cookie", Type = typeof(InetBankCookie))]
        [XmlArray(ElementName = "cookies")]
        public InetBankCookie[] CookieCollection { get; set; }
        /// <summary>
        /// Body for POST request
        /// </summary>
        [XmlElement(ElementName = "postData")]
        public string PostData { get; set; }
    }

    [Serializable]
    public class RequestHeaders
    {
        [XmlElement(ElementName = "contentType")]
        public string ContentType { get; set; }
        [XmlElement(ElementName = "preAuthenticate")]
        public bool PreAuthenticate { get; set; }
        [XmlElement(ElementName = "host")]
        public string Host { get; set; }
        [XmlElement(ElementName = "userAgent")]
        public string UserAgent { get; set; }
        [XmlElement(ElementName = "accept")]
        public string Accept { get; set; }
        [XmlElement(ElementName = "acceptLanguage")]
        public string AcceptLanguage { get; set; }
        [XmlElement(ElementName = "acceptEncoding")]
        public string AcceptEncoding { get; set; }
        [XmlElement(ElementName = "acceptCharset")]
        public string AcceptCharset { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "internetBanks")]
    public class InternetBanks
    {
        [XmlElement(ElementName = "bank")]
        public InetBank[] Banks { get; set; }
    }

    [Serializable]
    public class InetBank
    {
        /// <summary>
        /// Bank ID in Naviam database
        /// </summary>
        [XmlAttribute(AttributeName = "naviamId")]
        public string NaviamId { get; set; }
        /// <summary>
        /// Internet Bank ID
        /// </summary>
        [XmlAttribute(AttributeName = "iBankId")]
        public string BankId { get; set; }
        /// <summary>
        /// Internet Bank Settings
        /// </summary>
        [XmlAttribute(AttributeName = "iBankSettings")]
        public string BankSettings { get; set; }
    }
}
