using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Naviam.InternetBank.Entities;
using ScrapySharp.Extensions;
using log4net;

namespace Naviam.InternetBank.Helpers
{
    public class SbsibankHtmlParser
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SbsibankHtmlParser));

        public static IEnumerable<PaymentCard> ParseCardList(string selector, StreamReader content)
        {
            var cards = new List<PaymentCard>();
            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(content);
            var html = htmlDocument.DocumentNode;

            var cardsHtml = html.CssSelect(selector);
            foreach (var cardHtml in cardsHtml.Where(card => card.HasAttributes))
            {
                var card = new PaymentCard
                               {
                                   Id =
                                       cardHtml.Attributes["Value"] != null
                                           ? cardHtml.Attributes["Value"].Value
                                           : String.Empty,
                                   Name = cardHtml.NextSibling.CssSelect("i").First().InnerText
                               };

                Log.InfoFormat("Card name: {0}", card.Name);

                if (card.Id != null)
                {
                    Log.InfoFormat("Card id: {0}", card.Id);
                }
                cards.Add(card);
            }
            return cards;
        }

        public static void ParseBalance(string selector, StreamReader content, ref PaymentCard card)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(content);
            var html = htmlDocument.DocumentNode;

            var balanceHtml = html.CssSelect(selector).First().InnerHtml;
            var balanceArray = balanceHtml.Replace("&nbsp;&nbsp;", "-").Split('-');

            var balance = String.Join(null, Regex.Split(balanceArray[0], "[^\\d]"));
            card.Balance = Decimal.Parse(balance);
            card.Currency = balanceArray[1];
        }

        public static void ParseCardHistory(string selector, StreamReader content, ref PaymentCard card)
        {
            var htmlDocument = new HtmlDocument();
            
            htmlDocument.Load(content);
            var html = htmlDocument.DocumentNode;

            var historyRow = html.CssSelect(selector).ElementAt(1);

            if (historyRow != null)
            {
                // get status
                var statusElement = historyRow.CssSelect("td").ElementAt(1).CssSelect("b > font").FirstOrDefault(); //:eq(1) > b > font
                if (statusElement != null)
                {
                    card.Status = statusElement.InnerText.Trim();
                }

                // get register date
                var registerDateElement = historyRow.CssSelect("td").ElementAt(2);
                if (registerDateElement != null)
                {
                    DateTime registerDate;
                    DateTime.TryParse(registerDateElement.InnerText.Trim(), out registerDate);
                    card.RegisterDate = registerDate;
                }

                // get cancel date
                var cancelDateElement = historyRow.CssSelect("td").ElementAt(3);
                if (cancelDateElement != null)
                {
                    DateTime cancelDate;
                    DateTime.TryParse(cancelDateElement.InnerText.Trim(), out cancelDate);
                    card.CancelDate = cancelDate;
                }
            }
        }

        public static IEnumerable<AccountTransaction> ParseLatestTransactions(string selector, StreamReader content)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(content);
            var html = htmlDocument.DocumentNode;

            var reportRows = html.CssSelect(selector).Skip(1);
            return reportRows.Select(row => row.CssSelect("td")).Select(
                columns => new AccountTransaction
                {
                    OperationDate = DateTime.Parse(columns.ElementAt(0).InnerText.Trim(), CultureInfo.CreateSpecificCulture("ru-RU")),
                    Status = columns.ElementAt(1).InnerText.Trim(),
                    TransactionAmount = Decimal.Parse(columns.ElementAt(2).InnerText.Trim().Replace("&nbsp;", "")),
                    Currency = columns.ElementAt(3).InnerText.Trim(),
                    OperationDescription = columns.ElementAt(4).InnerText.Trim().Replace("&nbsp;", " ")
                }); 
        }

        public static IEnumerable<ReportPeriod> ParseStatementsList(StreamReader content)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(content);
            var html = htmlDocument.DocumentNode;

            var reportRows = html.CssSelect("p[class=mainfnt] > table[class=mainfnt] > tr").Skip(1);
            return reportRows.Select(row => row.CssSelect("td")).Select(
                columns => new ReportPeriod
                            {
                                StartDate = DateTime.Parse(columns.ElementAt(0).InnerText.Trim()), 
                                EndDate = DateTime.Parse(columns.ElementAt(1).InnerText.Trim()), 
                                CreatedDate = DateTime.Parse(columns.ElementAt(2).InnerText.Trim()), 
                                Id = String.Join(null, Regex.Split(columns.ElementAt(3).CssSelect("a").FirstOrDefault().Attributes["href"].Value, "[^\\d]"))
                            });
        }

        public static Report ParseReport(string selector, StreamReader content)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(content);
            var html = htmlDocument.DocumentNode;

            var currency = html.CssSelect("tr:nth-child(11) tt");
            var cardNumber = html.CssSelect("tr:nth-child(9) b tt");
            var startDate = html.CssSelect("tr:nth-child(14) b tt");
            var endDate = html.CssSelect("font:nth-child(4) tt");
            var report = new Report();
            var trans = new List<AccountTransaction>();
            var records = html.CssSelect("tr").Where(r => r.InnerHtml.Contains("<td width=\"55\" colspan=\"3\" rowspan=\"2\">"));

            foreach (var values in records.Select(record => record.CssSelect("tt")))
            {
                trans.Add(new AccountTransaction
                              {
                                  Currency = values.ElementAt(3).InnerText.Trim(),
                                  OperationDescription = values.ElementAt(1).InnerText.Trim().Replace("&nbsp;", " "),
                                  OperationDate = DateTime.Parse(values.ElementAt(0).InnerText.Trim(), CultureInfo.CreateSpecificCulture("ru-RU")),
                                  TransactionAmount = Decimal.Parse(values.ElementAt(2).InnerText.Trim().Replace("&nbsp;", ""))
                              });
                //Log.InfoFormat("Record - Date {0} : Amount: {1} Description : {2}",
                //               values.ElementAt(0).InnerText.Trim(), 
                //               values.ElementAt(2).InnerText.Trim(), 
                //               values.ElementAt(1).InnerText.Trim());
            }
            report.Transactions = trans;
            return report;
        }
    }
}
