using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

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

        private void PrepareHeaders()
        {
            
        }

        public static dynamic GetRequestUri(string bankId)
        {
            switch (bankId)
            {
                case "0175":
                    return new
                               {
                                   LoginUri = "login.asp?mode=1"
                               };
            }
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
            string cookies;

            using (var client = new HttpClient())
            {
                var content = new StringContent("S1=0175&T1=FQ529&C1=ON&T2=XUI4K&B1=%C4%E0%EB%E5%E5");
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/x-ms-application"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xaml+xml"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-ms-xbap"));
                client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");

                Console.WriteLine("Openning " + "https://www.sbsibank.by/login.asp?mode=1" + " with content: " + content);

                var reponse = client.Post("https://www.sbsibank.by/login.asp?mode=1", content);

                Console.WriteLine("Received status code: " + reponse.StatusCode.ToString());

                if (reponse.StatusCode == HttpStatusCode.Found)
                {
                    // ok

                    var setcookies = reponse.Headers.GetValues("Set-Cookie");
                    cookies = setcookies.ToString();
                    Console.WriteLine("Cookies: " + cookies);

                    var reportRequestContent = new StringContent("FromDate=01%2F10%2F2011&ToDate=17%2F10%2F2011");
                    Console.WriteLine("Openning " + "https://www.sbsibank.by/addreptask.asp" + " with content: " +
                                      reportRequestContent);
                    var responseReportRequest = client.Post("https://www.sbsibank.by/addreptask.asp",
                                                            reportRequestContent);

                    var location = responseReportRequest.Headers.Location;
                    Console.WriteLine("Openning " + "https://www.sbsibank.by/show.asp?id=106874" + " with content: " +
                                      content2);
                    var response2 = client.Post("https://www.sbsibank.by/show.asp?id=106874", content2);
                }
            }
        }
    }
}
