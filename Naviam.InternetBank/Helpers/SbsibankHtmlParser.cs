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
                    OperationAmount = Decimal.Parse(columns.ElementAt(2).InnerText.Trim().Replace("&nbsp;", "")),
                    Currency = columns.ElementAt(3).InnerText.Trim(),
                    OperationDescription = columns.ElementAt(4).InnerText.Trim().Replace("&nbsp;", " ")
                }); 
        }

        public static IEnumerable<ReportPeriod> ParseStatementsList(string selector, StreamReader content)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(content);
            var html = htmlDocument.DocumentNode;

            var reportRows = html.CssSelect(selector).Skip(1);
            return reportRows.Select(row => row.CssSelect("td")).Select(
                columns => new ReportPeriod
                            {
                                StartDate = DateTime.Parse(columns.ElementAt(0).InnerText.Trim()), 
                                EndDate = DateTime.Parse(columns.ElementAt(1).InnerText.Trim()), 
                                CreatedDate = DateTime.Parse(columns.ElementAt(2).InnerText.Trim()), 
                                Id = String.Join(null, Regex.Split(columns.ElementAt(3).CssSelect("a").First().Attributes["href"].Value, "[^\\d]"))
                            });
        }

        public static Report ParseReport(string selector, StreamReader content)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(content);
            var html = htmlDocument.DocumentNode;

            var report = new Report();
            var trans = new List<AccountTransaction>();
            // <td width=\"55\" colspan=\"3\" rowspan=\"2\"> <td width=361 colspan=26 rowspan=2>
            var reportPeriodRows = html.CssSelect("tr")
                .Where(r => (r.InnerHtml.Contains("<td width=\"361\" colspan=\"26\" rowspan=\"2\">") && r.InnerHtml.Contains("<td height=\"9\" colspan=\"22\">"))
                || (r.InnerHtml.Contains("<td width=\"361\" colspan=\"16\" rowspan=\"2\">") && r.InnerHtml.Contains("<td height=\"9\" colspan=\"15\">"))
                || (r.InnerHtml.Contains("<td width=\"361\" colspan=\"31\" rowspan=\"2\">") && r.InnerHtml.Contains("<td height=\"9\" colspan=\"29\">")));
            var reportPeriodCells = reportPeriodRows.Select(record => record.CssSelect("tt"));
            var period = new ReportPeriod();

            foreach (var reportPeriodCell in reportPeriodCells)
            {
                if (reportPeriodCell == null) continue;
                var local = reportPeriodCell.ToList();
                if (!local.Any()) continue;
                var count = local.Count();
                if (count == 4)
                {
                    if (local[0].InnerHtml == "Отчетный&nbsp;период:&nbsp;")
                    {
                        period.StartDate = DateTime.Parse(local[1].InnerText.Trim(), CultureInfo.CreateSpecificCulture("ru-RU"));
                        period.EndDate = DateTime.Parse(local[3].InnerText.Trim(), CultureInfo.CreateSpecificCulture("ru-RU"));
                        report.ReportPeriod = period;
                    }
                }
            }

            var cardRows = html.CssSelect("tr")
                .Where(r => r.InnerHtml.Contains("<td width=\"284\" colspan=\"10\" rowspan=\"2\">"));
            var cardCells = cardRows.Select(record => record.CssSelect("tt"));
            foreach (var cardCell in cardCells)
            {
                if (cardCell == null) continue;
                var local = cardCell.ToList();
                if (!local.Any()) continue;
                var count = local.Count();
                if (count == 2)
                {
                    if (local[0].InnerHtml == "Контракт&nbsp;#&nbsp;")
                        report.CardNumber = local[1].InnerHtml.Replace("&nbsp;", " ").Trim();
                }
            }

            var stateRows = html.CssSelect("tr")
                .Where(r => r.InnerHtml.Contains("<td height=\"9\"></td>") && 
                    (r.InnerHtml.Contains("<td width=\"100\" colspan=\"10\">") || (r.InnerHtml.Contains("<td width=\"100\" colspan=\"4\">"))));
            var stateCells = stateRows.Select(record => record.CssSelect("tt"));
            var stateOnDate = new StateOnDate();
            foreach (var stateCell in stateCells)
            {
                if (stateCell == null) continue;
                var local = stateCell.ToList();
                if (!local.Any()) continue;
                var count = local.Count();
                if (count == 4)
                {
                    if (local[0].InnerHtml != "На&nbsp;Дату")
                    {
                        stateOnDate.Date = DateTime.Parse(local[0].InnerText.Trim(), CultureInfo.CreateSpecificCulture("ru-RU"));
                        stateOnDate.Available = Decimal.Parse(local[1].InnerText.Replace("&nbsp;", "").Trim());
                        stateOnDate.BlockedAmount = Decimal.Parse(local[2].InnerText.Replace("&nbsp;", "").Trim());
                        stateOnDate.CreditLimit = Decimal.Parse(local[3].InnerText.Replace("&nbsp;", "").Trim());
                        report.StateOnDate = stateOnDate;
                    }
                }
            }

            var transcationRows = html.CssSelect("tr")
                .Where(r => r.InnerHtml.Contains("<td height=\"9\" colspan=\"2\"></td>")
                    || r.InnerHtml.Contains("<td height=\"9\" colspan=\"28\">")
                    || r.InnerHtml.Contains("<td width=\"69\" colspan=\"2\" rowspan=\"2\">"));
            var transactionCells = transcationRows.Select(record => record.CssSelect("tt"));

            var transactionDate = DateTime.MinValue; // fixed bug with html report from sbsibank (the transaction date is on another row)

            foreach (var values in transactionCells)
            {
                if (values == null) continue;
                var local = values.ToList();
                if (!local.Any()) continue;
                var count = local.Count();
                switch (count)
                {
                    case 1:
                        transactionDate = DateTime.Parse(local[0].InnerText.Trim(), CultureInfo.CreateSpecificCulture("ru-RU"));
                        break;
                    case 2:
                        if (local[0].InnerHtml == "Валюта&nbsp;Контракта:&nbsp;")
                            report.Currency = local[1].InnerText.Replace("&nbsp;", " ").Trim();
                        if (local[0].InnerHtml == "BYR" || local[0].InnerHtml == "USD" || local[0].InnerHtml == "EUR")
                            report.StartBalance = Decimal.Parse(local[1].InnerText.Replace("&nbsp;", " ").Replace(" ", "").Trim());
                        break;
                    case 3:
                        if (local[0].InnerHtml == "Операции&nbsp;по&nbsp;Card&nbsp;#&nbsp;")
                            report.CardNumber = local[1].InnerHtml.Replace("&nbsp;", " ").Trim();
                        break;
                    case 6:
                        if (local[0].InnerText.Trim() == "Дата") continue;
                        var transaction1 = new AccountTransaction
                        {
                            OperationDate = DateTime.Parse(local[0].InnerText.Trim(), CultureInfo.CreateSpecificCulture("ru-RU")),
                            OperationDescription = HttpUtility.HtmlDecode(local[1].InnerText.Replace("&nbsp;", " ").Trim()),
                            OperationAmount = Decimal.Parse(local[2].InnerText.Replace("&nbsp;", "").Trim()),
                            Currency = local[3].InnerText.Trim(),
                            TransactionDate = transactionDate,
                            Commission = Decimal.Parse(local[4].InnerText.Replace("&nbsp;", "").Trim()),
                            TransactionAmount = Decimal.Parse(local[5].InnerText.Replace("&nbsp;", "").Trim()),
                            Status = "F"
                        };
                        Log.Info(transaction1);
                        trans.Add(transaction1);
                        break;
                    case 7:
                        if (local[0].InnerText.Trim() == "Дата") continue;
                        DateTime date;
                        if (DateTime.TryParse(local[1].InnerText.Trim(), out date))
                        {
                            // operations waiting authorization
                            var transaction3 = new AccountTransaction
                            {
                                OperationDate = DateTime.Parse(local[0].InnerText.Trim(), CultureInfo.CreateSpecificCulture("ru-RU")),
                                OperationDescription = HttpUtility.HtmlDecode(local[2].InnerText.Replace("&nbsp;", " ").Trim()),
                                OperationAmount = Decimal.Parse(local[3].InnerText.Replace("&nbsp;", "").Trim()),
                                Currency = local[4].InnerText.Trim(),
                                //TransactionDate = DateTime.Parse(local[4].InnerText.Trim(), CultureInfo.CreateSpecificCulture("ru-RU")),
                                //Commission = Decimal.Parse(local[5].InnerText.Replace("&nbsp;", "").Trim()),
                                TransactionAmount = Decimal.Parse(local[5].InnerText.Replace("&nbsp;", "").Trim()),
                                Status = "A"
                            };
                            Log.Info(transaction3);
                            trans.Add(transaction3);
                            break;
                        }
                        var transaction2 = new AccountTransaction
                        {
                            OperationDate = DateTime.Parse(local[0].InnerText.Trim(), CultureInfo.CreateSpecificCulture("ru-RU")),
                            OperationDescription = HttpUtility.HtmlDecode(local[1].InnerText.Replace("&nbsp;", " ").Trim()),
                            OperationAmount = Decimal.Parse(local[2].InnerText.Replace("&nbsp;", "").Trim()),
                            Currency = local.ElementAt(3).InnerText.Trim(),
                            TransactionDate = DateTime.Parse(local[4].InnerText.Trim(), CultureInfo.CreateSpecificCulture("ru-RU")),
                            Commission = Decimal.Parse(local[5].InnerText.Replace("&nbsp;", "").Trim()),
                            TransactionAmount = Decimal.Parse(local[6].InnerText.Replace("&nbsp;", "").Trim()),
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
