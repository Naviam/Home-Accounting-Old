using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
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

            var startDate = html.CssSelect("tr:nth-child(14) b tt");
            var endDate = html.CssSelect("font:nth-child(4) tt");
            var report = new Report();
            var trans = new List<AccountTransaction>();
            // <td width=\"55\" colspan=\"3\" rowspan=\"2\">
            var records = html.CssSelect("tr").Where(r => r.InnerHtml.Contains("<td height=\"9\" colspan=\"2\"></td>") || r.InnerHtml.Contains("<td height=\"9\" colspan=\"28\">"));
            var transactionRecords = records.Select(record => record.CssSelect("tt"));

            DateTime transactionDate = DateTime.MinValue; // fixed bug with html report from sbsibank (the transaction date is on another row)

            foreach (var values in transactionRecords)
            {
                if (values == null) continue;
                var local = values.ToList();
                if (!local.Any()) continue;
                var count = local.Count();
                switch (count)
                {
                    case 1:
                        transactionDate = DateTime.Parse(local.ElementAt(0).InnerText.Trim(),
                                                         CultureInfo.CreateSpecificCulture("ru-RU"));
                        break;
                    case 2:
                        if (local[0].InnerHtml == "Валюта&nbsp;Контракта:&nbsp;")
                            report.Currency = local[1].InnerText.Replace("&nbsp;", " ").Trim();
                        if (local[0].InnerHtml == "BYR" || local[0].InnerHtml == "USD" || local[0].InnerHtml == "EUR")
                            report.StartBalance = Decimal.Parse(local[1].InnerText.Replace("&nbsp;", " ").Replace(" ", "").Trim());
                        break;
                    case 3:
                        if (local[0].InnerHtml == "Операции&nbsp;по&nbsp;Card&nbsp;#&nbsp;")
                        {
                            report.CardNumber = local[1].InnerHtml.Replace("&nbsp;", " ").Trim();
                        }
                        break;
                    case 6:
                        if (local.ElementAt(0).InnerText.Trim() == "Дата") continue;
                        var transaction1 = new AccountTransaction
                                              {
                                                  OperationDate = DateTime.Parse(local.ElementAt(0).InnerText.Trim(), CultureInfo.CreateSpecificCulture("ru-RU")),
                                                  OperationDescription = HttpUtility.HtmlDecode(local.ElementAt(1).InnerText.Replace("&nbsp;", " ").Trim()),
                                                  OperationAmount = Decimal.Parse(local.ElementAt(2).InnerText.Replace("&nbsp;", "").Trim()),
                                                  Currency = local.ElementAt(3).InnerText.Trim(),
                                                  TransactionDate = transactionDate,
                                                  Commission = Decimal.Parse(local.ElementAt(4).InnerText.Replace("&nbsp;", "").Trim()),
                                                  TransactionAmount = Decimal.Parse(local.ElementAt(5).InnerText.Replace("&nbsp;", "").Trim()),
                                                  Status = "F"
                                              };
                        Log.Info(transaction1);
                        trans.Add(transaction1);
                        break;
                    case 7:
                        if (local.ElementAt(0).InnerText.Trim() == "Дата") continue;
                        var transaction2 = new AccountTransaction
                                              {
                                                  OperationDate = DateTime.Parse(local.ElementAt(0).InnerText.Trim(), CultureInfo.CreateSpecificCulture("ru-RU")),
                                                  OperationDescription = HttpUtility.HtmlDecode(local.ElementAt(1).InnerText.Replace("&nbsp;", " ").Trim()),
                                                  OperationAmount = Decimal.Parse(local.ElementAt(2).InnerText.Replace("&nbsp;", "").Trim()),
                                                  Currency = local.ElementAt(3).InnerText.Trim(),
                                                  TransactionDate = DateTime.Parse(local.ElementAt(4).InnerText.Trim(), CultureInfo.CreateSpecificCulture("ru-RU")),
                                                  Commission = Decimal.Parse(local.ElementAt(5).InnerText.Replace("&nbsp;", "").Trim()),
                                                  TransactionAmount = Decimal.Parse(local.ElementAt(6).InnerText.Replace("&nbsp;", "").Trim()),
                                                  Status = "F"
                                              };
                        Log.Info(transaction2);
                        trans.Add(transaction2);
                        break;
                }

                
            }
            report.Transactions = trans;
            return report;
        }
    }
}
