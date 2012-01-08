using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Naviam.InternetBank.Entities;

namespace Naviam.InternetBank.Helpers
{
    public sealed class BankUtility
    {
        public static IEnumerable<ReportPeriod> GetReportPeriods(
            DateTime startDate, DateTime endDate, int maxDaysPeriod, List<ReportPeriod> pregeneratedReports)
        {
            var reports = new List<ReportPeriod>();
            //var reportsToCreate = new List<ReportPeriod>();
            ReportPeriod report = null;

            do
            {
                if (startDate > endDate) continue;

                report = pregeneratedReports.Where(r => r.StartDate <= startDate && r.EndDate > startDate)
                            .OrderByDescending(r => r.EndDate).FirstOrDefault() ??
                        pregeneratedReports.Where(r => r.StartDate > startDate).OrderBy(r => r.StartDate)
                            .FirstOrDefault();

                if (report == null) continue;

                report.IsCreated = true;

                if (report.StartDate > startDate)
                {
                    var start = startDate;
                    do
                    {
                        if (DaysBetween(report.StartDate, start) > maxDaysPeriod)
                        {
                            var end = start.AddDays(maxDaysPeriod);
                            var createReport = new ReportPeriod
                            {
                                StartDate = start,
                                EndDate = end,
                                IsCreated = false
                            };
                            reports.Add(createReport);
                            start = end.AddDays(1);
                        }
                        else
                        {
                            var createReport = new ReportPeriod
                            {
                                StartDate = start,
                                EndDate = report.StartDate.AddDays(-1),
                                IsCreated = false
                            };
                            reports.Add(createReport);
                            start = report.StartDate.AddDays(1);
                        }

                    } while (start < report.StartDate);
                }
                reports.Add(report);
                startDate = report.EndDate.AddDays(1);
            } while (report != null);

            var start2 = startDate;
            do
            {
                if (DaysBetween(endDate, start2) > maxDaysPeriod)
                {
                    var end = start2.AddDays(maxDaysPeriod);
                    var createReport = new ReportPeriod
                    {
                        StartDate = start2,
                        EndDate = end,
                        IsCreated = false
                    };
                    reports.Add(createReport);
                    start2 = end.AddDays(1);
                }
                else
                {
                    var createReport = new ReportPeriod
                    {
                        StartDate = start2,
                        EndDate = endDate,
                        IsCreated = false
                    };
                    reports.Add(createReport);
                    start2 = endDate;
                }

            } while (start2 < endDate);

            return reports;
        }

        public static int DaysBetween(DateTime d1, DateTime d2)
        {
            var span = d2.Subtract(d1);
            return Math.Abs((int)span.TotalDays);
        }

        public static HttpWebRequest GetRequest(InetBankRequest request, CookieCollection cookies, Uri baseUri, 
            RequestHeaders headers, bool followRedirect = true, string method = "GET")
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(GetAbsoluteUri(baseUri, request.Url));
            // allows for validation of SSL conversations
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            AddCommonHeadersToHttpRequest(webRequest, cookies, headers, baseUri, request.Referer, method, followRedirect);
            return webRequest;
        }

        // callback used to validate the certificate in an SSL conversation
        private static bool ValidateRemoteCertificate(
            object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return cert.Subject.ToUpper().Contains("SBSIBANK");
        }

        public static void AddCommonHeadersToHttpRequest(
            HttpWebRequest request, CookieCollection cookies, RequestHeaders headers, Uri baseUri,
            string referer, string method, bool followRedirect = true)
        {
            // add cookies to request
            var cookieContainer = new CookieContainer();
            if (cookies != null) cookieContainer.Add(cookies);
            request.CookieContainer = cookieContainer;

            request.Method = method;
            request.AllowAutoRedirect = followRedirect;
            request.KeepAlive = true;
            request.ContentType = headers.ContentType;
            request.PreAuthenticate = headers.PreAuthenticate;
            request.Host = headers.Host;
            request.UserAgent = headers.UserAgent;
            request.Accept = headers.Accept;
            request.Headers.Add("Accept-Language", headers.AcceptLanguage);
            request.Headers.Add("Accept-Encoding", headers.AcceptEncoding);
            request.Headers.Add("Accept-Charset", headers.AcceptCharset);

            Uri uriResult;
            if (Uri.TryCreate(baseUri, referer, out uriResult))
                request.Referer = uriResult.AbsoluteUri;
        }

        public static string GetAbsoluteUri(Uri baseUri, string relativeUri)
        {
            Uri resultUri;
            return Uri.TryCreate(baseUri, relativeUri, out resultUri) ? resultUri.AbsoluteUri : String.Empty;
        }
    }
}
