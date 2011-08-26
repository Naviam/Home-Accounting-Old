using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SMSServer
{
    public class SMSInfo
    {
        public SMSInfo(){ }
        public SMSInfo(Match match)
        {
            Number = match.Groups["cardnumber"].Success ? match.Groups["cardnumber"].Value : "";
            Operation = match.Groups["operation"].Success ? match.Groups["operation"].Value : "";
            Result = match.Groups["result"].Success ? match.Groups["result"].Value : "";
            DateTime dt;
            DateTime.TryParse(match.Groups["datetime"].Success ? match.Groups["datetime"].Value : DateTime.MinValue.ToString(), out dt);
            Date = dt;
            decimal amount;
            Decimal.TryParse(match.Groups["amount"].Success ? match.Groups["amount"].Value : "0", out amount);
            Amount = amount;
            ShortCurrency = match.Groups["amount_currency"].Success ? match.Groups["amount_currency"].Value : "";
        }

        public string Number { get; set; }
        public string Operation { get; set; }
        public string Result { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string ShortCurrency { get; set; }
    }

    class Program
    {
        static string s = @"
4..0692
Retail
Uspeshno
2011-08-22 09:32:37
Summa: 50000 BYR
Ostatok: 141380 BYR
Na vremya: 09:32:43
BLR/MINSK/BELCEL I-BANK
";
        
        static string regex = @"\d\.\.(?<cardnumber>\d{4})?\r\n(?<operation>[^\x00]*?)\r\n(?<result>[^\x00]*?)\r\n(?<datetime>[^\x00]*?)\r\nSumma\:(?<amount>[^\x00]*?)\s(?<amount_currency>[^\x00]{3})\r\n"; 
        static void Main(string[] args)
        {
            Regex reg = new Regex(regex, RegexOptions.Multiline | RegexOptions.CultureInvariant);
            if (reg.IsMatch(s))
            {
                Match m = reg.Match(s);
                SMSInfo info = new SMSInfo(m);
            }

        }
    }
}
