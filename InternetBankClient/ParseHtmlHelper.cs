using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using log4net;

namespace InternetBankClient
{
    public class ReportRow
    {
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Id { get; set; }
        public bool IsCreated { get; set; }

        public override string ToString()
        {
            return String.Format("Range {0} - {1} is {2}",
                PeriodStartDate.Date.ToShortDateString(), PeriodEndDate.Date.ToShortDateString(), IsCreated ? "created" : "not created");
        }
    }

    public class PaymentCard
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Currency { get; set; }
        public decimal Balance { get; set; }
        public DateTime RegisterDate { get; set; }
        public DateTime CancelDate { get; set; }
        public string Status { get; set; }
    }

    internal class ParseHtmlHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ParseHtmlHelper));

        public static IEnumerable<PaymentCard> ParseCardList(StreamReader content)
        {
            var cards = new List<PaymentCard>();
            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(content);
            var html = htmlDocument.DocumentNode;

            var cardsHtml = html.CssSelect("input[type=radio][name=R1]");
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

        public static void ParseBalance(StreamReader content, out string balance, out string currency)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(content);
            var html = htmlDocument.DocumentNode;

            var balanceHtml = html.CssSelect("p[class=mainfnt]").CssSelect("b").CssSelect("font[color=red]").First().InnerHtml;
            var balanceArray = balanceHtml.Replace("&nbsp;&nbsp;", " ").Split(' ');

            balance = balanceArray[0];
            currency = balanceArray[1];

            Log.InfoFormat("Balance: {0} and currency: {1}", balance, currency);
        }

        public static void ParseCardHistory(StreamReader content, out string status, out DateTime registerDate, out DateTime cancelDate)
        {
            var htmlDocument = new HtmlDocument();
            
            htmlDocument.Load(content);
            var html = htmlDocument.DocumentNode;
            status = null;
            registerDate = DateTime.MinValue;
            cancelDate = DateTime.MinValue;

            var historyRow = html.CssSelect("table[cellspacing=5]").CssSelect("tr").ElementAt(1);

            if (historyRow != null)
            {
                // get status
                var statusElement = historyRow.CssSelect("td").ElementAt(1).CssSelect("b > font").FirstOrDefault(); //:eq(1) > b > font
                if (statusElement != null)
                {
                    status = statusElement.InnerText.Trim();
                }

                // get register date
                var registerDateElement = historyRow.CssSelect("td").ElementAt(2);
                if (registerDateElement != null)
                {
                    DateTime.TryParse(registerDateElement.InnerText.Trim(), out registerDate);

                }

                // get cancel date
                var cancelDateElement = historyRow.CssSelect("td").ElementAt(3);
                if (cancelDateElement != null)
                {
                    DateTime.TryParse(cancelDateElement.InnerText.Trim(), out cancelDate);
                }
            }
        }

        public static IEnumerable<ReportRow> ParseStatementsList(StreamReader content)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(content);
            var html = htmlDocument.DocumentNode;

            var reportRows = html.CssSelect("p[class=mainfnt] > table[class=mainfnt] > tr").Skip(1);
            return reportRows.Select(row => row.CssSelect("td")).Select(
                columns => new ReportRow
                            {
                                PeriodStartDate = DateTime.Parse(columns.ElementAt(0).InnerText.Trim()), 
                                PeriodEndDate = DateTime.Parse(columns.ElementAt(1).InnerText.Trim()), 
                                CreatedDate = DateTime.Parse(columns.ElementAt(2).InnerText.Trim()), 
                                Id = String.Join(null, Regex.Split(columns.ElementAt(3).CssSelect("a").FirstOrDefault().Attributes["href"].Value, "[^\\d]"))
                            });
        }
    }
}
