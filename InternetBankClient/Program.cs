using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using HtmlAgilityPack;
using ScrapySharp.Extensions;

namespace InternetBankClient
{
    public class InternetBank : IDisposable
    {
        private readonly HttpClient _client;

        public InternetBank(string baseAddress)
        {
            if (_client == null)
                _client = new HttpClient();
            _client.BaseAddress = new Uri(baseAddress);
            _client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/x-ms-application"));
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xaml+xml"));
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-ms-xbap"));
            _client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
        }

        public static dynamic GetRequestUri(string bankId)
        {
                    return "login.asp?mode=1";
        }
    
        public void Login(string bankId, string username, string password)
        {
            // 0175 FQ529 XUI4K &C1=ON
            var content = new StringContent(
                String.Format("S1={0}&T1={1}&T2={2}&B1=%C4%E0%EB%E5%E5", bankId, username, password));
            var response = _client.Post(GetRequestUri(bankId).LoginUri, content);
            _client.DefaultRequestHeaders.Add("Cookie", response.Headers.GetValues("Set-Cookie"));
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var loginRequest = (HttpWebRequest)WebRequest.Create("https://www.sbsibank.by/login.asp");
            loginRequest.Method = "POST";
            var cookieContainer = new CookieContainer();
            loginRequest.CookieContainer = cookieContainer;
            loginRequest.ContentType = "application/x-www-form-urlencoded";
            var encoding = new ASCIIEncoding();
            const string postData = "S1=0175&T1=FQ529&C1=ON&T2=XUI4K&B1=%C4%E0%EB%E5%E5";
            var data = encoding.GetBytes(postData);

            loginRequest.ContentLength = postData.Length;
            Console.WriteLine("Openning " + "https://www.sbsibank.by/login.asp?mode=1" + " with content: " + postData);

            Stream dataStream = loginRequest.GetRequestStream();
            dataStream.Write(data, 0, data.Length);
            dataStream.Close();
            string result = string.Empty;
            
            var response = loginRequest.GetResponse();

            var responseStream = response.GetResponseStream();
            if (responseStream != null)
            {
                var reader = new StreamReader(responseStream, Encoding.UTF8);
                result = reader.ReadToEnd();
            }
            //Console.WriteLine("Result : " + result);

            var setcookie = response.Headers["Set-Cookie"];
            Console.WriteLine("Set-Cookie: " + setcookie);

            var cookieSet = setcookie.Substring(0, setcookie.IndexOf(';')).Split('=');
            var cookieHeader = cookieSet[0];
            var cookieValue = cookieSet[1];
            // new request

            var accountsRequest = (HttpWebRequest)WebRequest.Create("https://www.sbsibank.by/right.asp");
            accountsRequest.Method = "POST";
            var cookie = new CookieContainer();
            cookie.Add(new Cookie(cookieHeader, cookieValue, "/"));
            accountsRequest.CookieContainer = cookie;
            accountsRequest.ContentType = "application/x-www-form-urlencoded";

            //const string postData = "S1=0175&T1=FQ529&C1=ON&T2=XUI4K&B1=%C4%E0%EB%E5%E5";
            //var data = encoding.GetBytes(postData);

            loginRequest.ContentLength = 0; // postData.Length;
            Console.WriteLine("Openning " + "https://www.sbsibank.by/login.asp?mode=1" + " with content: " + postData);

            var rightDataStream = loginRequest.GetRequestStream();
            rightDataStream.Write(data, 0, data.Length);
            rightDataStream.Close();
            var result2 = string.Empty;

            var res = loginRequest.GetResponse();

            var resStream = response.GetResponseStream();
            if (resStream != null)
            {
                var reader = new StreamReader(resStream, Encoding.UTF8);
                result2 = reader.ReadToEnd();
            }

            //var client = new HttpClient {BaseAddress = new Uri("https://sbsibank.by")};
            //var request = new HttpRequestMessage(HttpMethod.Get, "right.asp");
            //request.Headers.Add("Cookie", setcookie);

            //var rightResponseMessage = client.Send(request);
            
            ////var cardHistory = client.Get("card_history.asp");
            ////var cardHistoryResponseMessage = cardHistory.EnsureSuccessStatusCode();
            ////var cardHistoryContent = cardHistoryResponseMessage.Content;

            //var htmlDocument = new HtmlDocument();
            //htmlDocument.Load(rightResponseMessage.Content.ContentReadStream);
            ////htmlDocument.Load(@"CardList.htm");
            //var html = htmlDocument.DocumentNode;

            //var cards = html.CssSelect("input[type=radio][name=R1]");
            //foreach (var card in cards.Where(card => card.HasAttributes))
            //{
            //    Console.WriteLine("Card name: " + card.NextSibling.OuterHtml);
            //    var cardId = card.Attributes["Value"];
            //    if (cardId != null)
            //    {
            //        Console.WriteLine("Card id: " + cardId.Value);
            //    }
            //}
            //using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            //{
            //    Console.WriteLine("Received status code: " + httpWebResponse.StatusCode);
            //    if (httpWebResponse.StatusCode == HttpStatusCode.OK)
            //    {
            //        cookies = httpWebResponse.Cookies;
            //        Console.WriteLine("Cookies: " + cookies[0]);
            //    }
            //}
                
            //using (var client = new HttpClient())
            //{
            //    var reportRequestContent = new StringContent("FromDate=01%2F10%2F2011&ToDate=17%2F10%2F2011");
            //    Console.WriteLine("Openning " + "https://www.sbsibank.by/addreptask.asp" + " with content: " +
            //                         reportRequestContent);
            //    var responseReportRequest = client.Post("https://www.sbsibank.by/addreptask.asp",
            //                                            reportRequestContent);
            //    var location = responseReportRequest.Headers.Location;
            //}
            Console.ReadLine();
        }
    }
}
