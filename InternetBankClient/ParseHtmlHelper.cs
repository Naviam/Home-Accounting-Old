using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using log4net;

namespace InternetBankClient
{
    public class PaymentCard
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Currency { get; set; }
        public decimal Balance { get; set; }
        public DateTime RegisteredDate { get; set; }
        public DateTime CancelationDate { get; set; }
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
    }
}
