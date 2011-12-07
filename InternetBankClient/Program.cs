﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using HtmlAgilityPack;
using RestSharp;
using ScrapySharp.Extensions;
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

            #region RestShart example
            ////Trust all certificates
            //ServicePointManager.ServerCertificateValidationCallback =
            //    ((sender, certificate, chain, sslPolicyErrors) => true);

            //// trust sender
            //ServicePointManager.ServerCertificateValidationCallback
            //                = ((sender, cert, chain, errors) => cert.Subject.Contains("sbsibank"));

            //// validate cert by calling a function
            //ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;

            //var client = new RestClient("https://sbsibank.by") { Authenticator = new SimpleAuthenticator("T1", "FQ529", "T2", "XUI4K") };

            //var request = new RestRequest("login.asp?mode=1", Method.POST);
            //request.AddHeader("Host", "www.sbsibank.by");
            //request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:8.0) Gecko/20100101 Firefox/8.0");
            //request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            //request.AddHeader("Accept-Language", "en-us,en;q=0.5");
            //request.AddHeader("Accept-Encoding", "gzip, deflate");
            //request.AddHeader("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
            ////request.AddHeader("Connection", "keep-alive");
            //request.AddHeader("Referer", "https://www.sbsibank.by/login.asp");
            //request.AddHeader("Cookie", "UN=FQ529; C1=checked; S1=0175; ASPSESSIONIDQACBBRTD=CGJPMDMCDPAOCANNGNJMKHNE; ASPSESSIONIDQABDAQSD=DCOHLMGDDEGAMHABLILICAGN; ASPSESSIONIDSAAADRSC=MADADOLANDGKDAJJPCJIAOFN; ASPSESSIONIDSACDARTC=AHMLOGGBGEIEGAKGNMHLBEPH");

            //request.AddBody("S1=0175&T1=FQ529&C1=ON&T2=XUI4K&B1=%C4%E0%EB%E5%E5");

            //var response = client.Execute(request);

            //return new[] {response.Cookies[0].Name, response.Cookies[0].Value}; 
            #endregion
        }

        public static HttpStatusCode Connect()
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
                return response.StatusCode;
            }
        }

        public static WebResponse GetHome()
        {
            // initialize home.asp get request
            var request = (HttpWebRequest)WebRequest.Create("https://www.sbsibank.by/home.asp");
            AddCommonHeadersToHttpRequest(request);
            request.Referer = "https://www.sbsibank.by/home.asp";

            // get response
            Log.Info("Openning https://www.sbsibank.by/right.asp ");
            var response = request.GetResponse();
            Log.Info("Right response: " + response);
            return response;
        }

        public static StreamReader GetRight()
        {
            // initialize right.asp get request
            var request = (HttpWebRequest)WebRequest.Create("https://www.sbsibank.by/right.asp");
            AddCommonHeadersToHttpRequest(request);
            request.Referer = "https://www.sbsibank.by/home.asp";
            
            // get response
            Log.Info("Openning https://www.sbsibank.by/right.asp");
            // get response
            //using ()
            //{
                var response = (HttpWebResponse) request.GetResponse();
                var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    //var responseText = new StreamReader(responseStream).ReadToEnd();
                    //Log.InfoFormat("right.asp response body is: {0}", responseText);
                    return new StreamReader(responseStream);
                }
                return null;
            //}
        }
    }

    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        public static void ParseCardList(StreamReader content)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(content);
            var html = htmlDocument.DocumentNode;

            var cards = html.CssSelect("input[type=radio][name=R1]");
            foreach (var card in cards.Where(card => card.HasAttributes))
            {
                Log.InfoFormat("Card name: {0}", card.NextSibling.OuterHtml);
                var cardId = card.Attributes["Value"];
                if (cardId != null)
                {
                    Log.InfoFormat("Card id: {0}", cardId.Value);
                }
            }
        }

        static void Main()
        {
            XmlConfigurator.Configure();
            SampleClient.GetLogin();
            if (SampleClient.Connect() == HttpStatusCode.OK)
            {
                var stream = SampleClient.GetRight();
                ParseCardList(stream);
            }
            Console.ReadLine();
        }
    }
}
