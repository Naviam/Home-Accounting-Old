using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Naviam.InternetBank.Entities;

namespace Naviam.InternetBank.Helpers
{
    public static class BankUtility
    {
        public static IEnumerable<ReportPeriod> GetReportsToCreate(DateTime startDate, DateTime endDate, int maxDaysInPeriod)
        {
            // check if end date is less than today
            endDate = DateTime.UtcNow <= endDate ? DateTime.UtcNow.AddDays(-1) : endDate;
            var reports = new List<ReportPeriod>();
            var nextStartDate = startDate;

            do
            {
                var periodStartDate = nextStartDate;
                nextStartDate = nextStartDate.AddDays(maxDaysInPeriod);
                if (nextStartDate > endDate) nextStartDate = endDate;
                nextStartDate = nextStartDate.AddDays(1);
                reports.Add(new ReportPeriod {StartDate = periodStartDate, EndDate = nextStartDate.AddDays(-1), Exists = false});
            } while (nextStartDate < endDate);
            
            return reports;
        }

        /// <summary>
        /// Tries to find report that intersect with or have the closest start date to the date.
        /// </summary>
        /// <param name="date">The date </param>
        /// <param name="reports">Reports that </param>
        /// <returns></returns>
        public static ReportPeriod FindBestIntersectReportWithDate(this IEnumerable<ReportPeriod> reports, DateTime date)
        {
            return (from r in reports
                   where r.StartDate <= date && r.EndDate > date
                   orderby r.EndDate descending
                   select r).FirstOrDefault();
        }

        public static ReportPeriod FindReportWithClosestStartDate(this IEnumerable<ReportPeriod> reports, DateTime date)
        {
            return (from r in reports
                    where r.StartDate >= date
                    orderby r.StartDate ascending 
                    select r).FirstOrDefault();
        }

        public static IEnumerable<ReportPeriod> GetReportPeriods(
            DateTime startDate, DateTime endDate, int maxDaysInPeriod, List<ReportPeriod> pregeneratedReports)
        {
            if (pregeneratedReports == null || !pregeneratedReports.Any())
                // TODO: check that exist reports are within set period
                return GetReportsToCreate(startDate, endDate, maxDaysInPeriod);

            // the rest works only if there are some reports generated on the internet bank website
            var result = new List<ReportPeriod>();
            var lastReportEndDate = startDate;
            // get best report for the date
            do
            {
                var nextReport = pregeneratedReports.FindBestIntersectReportWithDate(lastReportEndDate);
                if (nextReport != null)
                {
                    lastReportEndDate = nextReport.EndDate;
                    result.Add(nextReport);
                }
                else
                {
                    // get the report with the most close start date
                    nextReport = pregeneratedReports.FindReportWithClosestStartDate(lastReportEndDate);

                    if (nextReport != null)
                    {
                        // there is no exist reports
                        // create all reports
                        var reportsToCreate = GetReportsToCreate(
                            (startDate == lastReportEndDate) ? lastReportEndDate : lastReportEndDate.AddDays(1), 
                            nextReport.StartDate.AddDays(-1), maxDaysInPeriod);
                        result.AddRange(reportsToCreate);
                        lastReportEndDate = nextReport.EndDate;
                        result.Add(nextReport);
                    }
                }
            } while (lastReportEndDate < endDate);

            return result;

            //var reports = new List<ReportPeriod>();
            ////var reportsToCreate = new List<ReportPeriod>();
            //ReportPeriod report = null;

            //do
            //{
            //    if (startDate > endDate) continue;

            //    report = pregeneratedReports.Where(r => r.StartDate <= startDate && r.EndDate > startDate)
            //                .OrderByDescending(r => r.EndDate).FirstOrDefault() ??
            //            pregeneratedReports.Where(r => r.StartDate > startDate).OrderBy(r => r.StartDate)
            //                .FirstOrDefault();

            //    if (report == null) continue;

            //    report.Exists = true;

            //    if (report.StartDate > startDate)
            //    {
            //        var start = startDate;
            //        do
            //        {
            //            if (DaysBetween(report.StartDate, start) > maxDaysInPeriod)
            //            {
            //                var end = start.AddDays(maxDaysInPeriod);
            //                var createReport = new ReportPeriod
            //                {
            //                    StartDate = start,
            //                    EndDate = end,
            //                    Exists = false
            //                };
            //                reports.Add(createReport);
            //                start = end.AddDays(1);
            //            }
            //            else
            //            {
            //                var createReport = new ReportPeriod
            //                {
            //                    StartDate = start,
            //                    EndDate = report.StartDate.AddDays(-1),
            //                    Exists = false
            //                };
            //                reports.Add(createReport);
            //                start = report.StartDate.AddDays(1);
            //            }

            //        } while (start < report.StartDate);
            //    }
            //    reports.Add(report);
            //    startDate = report.EndDate.AddDays(1);
            //} while (report != null);

            //var start2 = startDate;
            //do
            //{
            //    if (DaysBetween(endDate, start2) > maxDaysInPeriod)
            //    {
            //        var end = start2.AddDays(maxDaysInPeriod);
            //        var createReport = new ReportPeriod
            //        {
            //            StartDate = start2,
            //            EndDate = end,
            //            Exists = false
            //        };
            //        reports.Add(createReport);
            //        start2 = end.AddDays(1);
            //    }
            //    else
            //    {
            //        var createReport = new ReportPeriod
            //        {
            //            StartDate = start2,
            //            EndDate = endDate,
            //            Exists = false
            //        };
            //        reports.Add(createReport);
            //        start2 = endDate;
            //    }

            //} while (start2 < endDate);

            //return reports;
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
        public static bool ValidateRemoteCertificate(
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
