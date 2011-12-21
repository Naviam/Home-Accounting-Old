using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;

namespace Naviam.InternetBank
{
    public class BankClient : IDisposable
    {
        private CookieCollection _cookies;
        private readonly Uri _baseUri;
        private readonly int _bankId;
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
            _bankId = bankId;
            // deserialize xml bank settings to object 
            // and read internet bank settings for specified bank id 
            var banks = LoadBanks(BanksXmlFileName);
            InetBank = (from b in banks
                       where b.NaviamId == bankId.ToString()
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
            Cookie setCookie;
            var responseCode = GetLoginPage(userName, InetBank.BankId, out setCookie);
            if (responseCode == 0 || responseCode == 3)
            {
                responseCode = Authenticate(userName, password, InetBank.BankId);
            }
            return new LoginResponse
                       {
                           ErrorCode = responseCode,
                           IsAuthenticated = responseCode == 0,
                           SetCookie = setCookie
                       };
        }

        /// <summary>
        /// Obtain the list of registered user's payment cards in internet bank system.
        /// </summary>
        public IEnumerable<PaymentCard> GetPaymentCards()
        {
            return new List<PaymentCard>();
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

        /// <summary>
        /// Open the Login page to get Set-Cookies response header 
        /// </summary>
        /// <returns>Response code.
        ///     0: Logged in without issues; 
        ///     1: Response status code is not OK;
        ///     2: Settings in XML has not been found for Login GET request;
        ///     3: Logged in fine. However, cookie collection in the XML settings was not found.
        /// </returns>
        private int GetLoginPage(string username, string iBankId, out Cookie setCookie)
        {
            setCookie = null;
            // get bank settings for get login request
            var loginGetRequest = Settings.LoginRequests
                .Where(lr => String.Compare(lr.Method, "GET", true) == 0)
                .FirstOrDefault();

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
                            setCookie = response.Cookies["Set-Cookie"];
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
        ///     1: Login Failed.
        /// </returns>
        public int Authenticate(string username, string password, string iBankId)
        {
            // get bank settings for get login request
            var loginPostRequest = Settings.LoginRequests
                .Where(lr => String.Compare(lr.Method, "POST", true) == 0)
                .FirstOrDefault();

            if (loginPostRequest != null)
            {
                var request = GetRequest(loginPostRequest.Url, loginPostRequest.Referer, "POST");

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
                    var responseStream = response.GetResponseStream();
                    if (responseStream != null)
                    {
                        var responseMessage = ParseHtmlHelper.ParseLoginResponse(
                            new StreamReader(responseStream, Encoding.GetEncoding(1251)));
                        if (responseMessage != null &&
                            String.Compare(responseMessage, "ВХОД В СИСТЕМУ", true) == 0)
                        {
                            return 1;
                        }
                    }
                    return response.StatusCode == HttpStatusCode.OK ? 0 : 1;
                }
            }
            return 2;
        }

        private HttpWebRequest GetRequest(string url, string referer, string method = "GET")
        {
            var request = (HttpWebRequest)WebRequest.Create(GetAbsoluteUri(url));
            AddCommonHeadersToHttpRequest(request, referer, method);
            return request;
        }

        private void AddCommonHeadersToHttpRequest(HttpWebRequest request, string referer, string method)
        {
            // add cookies to request
            var cookieContainer = new CookieContainer();
            if (_cookies != null) cookieContainer.Add(_cookies);
            request.CookieContainer = cookieContainer;

            request.Method = method;
            request.ContentType = Settings.RequestHeaders.ContentType;
            request.PreAuthenticate = Settings.RequestHeaders.PreAuthenticate;
            request.Host = Settings.RequestHeaders.Host;
            request.UserAgent = Settings.RequestHeaders.UserAgent;
            request.Accept = Settings.RequestHeaders.Accept;
            request.Headers.Add("Accept-Language", Settings.RequestHeaders.AcceptLanguage);
            request.Headers.Add("Accept-Encoding", Settings.RequestHeaders.AcceptEncoding);
            request.Headers.Add("Accept-Charset", Settings.RequestHeaders.AcceptCharset);

            request.Referer = referer;
        }

        private string GetAbsoluteUri(string relativeUri)
        {
            Uri resultUri;
            return Uri.TryCreate(_baseUri, relativeUri, out resultUri) ? resultUri.AbsoluteUri : String.Empty;
        }

        private InetBankSettings LoadSettings(string sbsibanksettingsXml)
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

        private IEnumerable<InetBank> LoadBanks(string fileName)
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
    }

    public class LoginResponse
    {
        public bool IsAuthenticated { get; set; }

        public int ErrorCode { get; set; }

        public Cookie SetCookie { get; set; }
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
    }

    public class InetBankRequest
    {
        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }
        [XmlElement(ElementName = "url", Type = typeof(string))]
        public string Url { get; set; }
        [XmlElement(ElementName = "referer", Type = typeof(string))]
        public string Referer { get; set; }
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
