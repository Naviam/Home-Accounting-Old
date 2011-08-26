using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SMSServer
{
    public abstract class SMSInfo
    {
        protected string _sms;
        protected string _cardNumber;
        protected string _operation;
        protected string _result;
        protected DateTime _date;
        protected decimal _amount;
        protected string _shortCurrency;
        protected string _merchant;
        
        public SMSInfo(string sms)
        { 
            _sms = sms;
            _cardNumber = GetCardNumber();
            _operation = GetOperation();
            _result = GetResult();
            _date = GetDate();
            _amount = GetAmount();
            _shortCurrency = GetShortCurrency();
            _merchant = GetMerchant();
        }

        public string CardNumber
        {
            get
            {
                return _cardNumber;
            }
        }
        public string Operation
        {
            get
            {
                return _operation;
            }
        }
        public string Result
        {
            get
            {
                return _result;
            }
        }
        public DateTime Date
        {
            get
            {
                return _date;
            }
        }
        public decimal Amount
        {
            get
            {
                return _amount;
            }
        }
        public string ShortCurrency
        {
            get
            {
                return _shortCurrency;
            }
        }
        public string Merchant
        {
            get
            {
                return _merchant;
            }
        }

        public virtual string GetCardNumber()
        {
            string result = string.Empty;
            string pattern = @"\d\.\.(?<cardnumber>\d{4}?)";
            Regex reg = new Regex(pattern, RegexOptions.Multiline | RegexOptions.CultureInvariant);
            if (reg.IsMatch(_sms))
            {
                Match match = reg.Match(_sms);
                result = match.Groups["cardnumber"].Success ? match.Groups["cardnumber"].Value : "";
            }
            return result;
        }

        public virtual string GetOperation()
        {
            string result = string.Empty;
            string pattern = @"(?<operation>Retail)";
            Regex reg = new Regex(pattern, RegexOptions.Multiline | RegexOptions.CultureInvariant);
            if (reg.IsMatch(_sms))
            {
                Match match = reg.Match(_sms);
                result = match.Groups["operation"].Success ? match.Groups["operation"].Value : "";
            }
            return result;
        }

        public virtual string GetResult()
        {
            string result = string.Empty;
            string pattern = @"(?<result>Uspeshno)";
            Regex reg = new Regex(pattern, RegexOptions.Multiline | RegexOptions.CultureInvariant);
            if (reg.IsMatch(_sms))
            {
                Match match = reg.Match(_sms);
                result = match.Groups["result"].Success ? match.Groups["result"].Value : "";
            }
            return result;
        }

        public virtual DateTime GetDate()
        {
            DateTime result = DateTime.MinValue;
            string pattern = @"(?<date>(?:\d{4}|\d{2})-\d{1,2}-\d{1,2}\s\d{1,2}:\d{1,2}:\d{1,5})";
            Regex reg = new Regex(pattern, RegexOptions.Multiline | RegexOptions.CultureInvariant);
            if (reg.IsMatch(_sms))
            {
                Match match = reg.Match(_sms);
                DateTime.TryParse(match.Groups["date"].Success ? match.Groups["date"].Value : DateTime.MinValue.ToString(), out result);
            }
            return result;
        }

        public virtual decimal GetAmount()
        {
            decimal result = 0;
            string pattern = @"Summa:\s?(?<amount>[^\x00]*?)\s";
            Regex reg = new Regex(pattern, RegexOptions.Multiline | RegexOptions.CultureInvariant);
            if (reg.IsMatch(_sms))
            {
                Match match = reg.Match(_sms);
                decimal.TryParse(match.Groups["amount"].Success ? match.Groups["amount"].Value : "0", out result);
            }
            return result;
        }

        public virtual string GetShortCurrency()
        {
            string result = string.Empty;
            string pattern = @"Summa:\s[^\x00]*?\s(?<currency>[^\x00]{3})";
            Regex reg = new Regex(pattern, RegexOptions.Multiline | RegexOptions.CultureInvariant);
            if (reg.IsMatch(_sms))
            {
                Match match = reg.Match(_sms);
                result = match.Groups["currency"].Success ? match.Groups["currency"].Value : "";
            }
            return result;
        }

        public virtual string GetMerchant()
        {
            string result = string.Empty;
            string pattern = @"[^\x00]*\r\n(?<merchant>[^\x00]*?\/[^\x00]*?\/[^\x00]*)\r\n";
            Regex reg = new Regex(pattern, RegexOptions.Multiline | RegexOptions.CultureInvariant);
            if (reg.IsMatch(_sms))
            {
                Match match = reg.Match(_sms);
                result = match.Groups["merchant"].Success ? match.Groups["merchant"].Value : "";
            }
            return result;
        }
    }

    public class SMSInfoBelswiss : SMSInfo
    {
        public SMSInfoBelswiss(string sms)
            : base(sms)
        {
            
        }

        //public override string GetCardNumber()
        //{
        //    string result = string.Empty;
        //    Regex reg = new Regex(_cardNumberRegex, RegexOptions.Multiline | RegexOptions.CultureInvariant);
        //    if (reg.IsMatch(_sms))
        //    {
        //        Match match = reg.Match(_sms);
        //        result = match.Groups["cardnumber"].Success ? match.Groups["cardnumber"].Value : "";
        //    }
        //    return result;
        //}
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
        //[^\x00]*\r\n(?<selector>[^\x00]*?\/[^\x00]*?\/[^\x00]*) merchant
        static void Main(string[] args)
        {
            SMSInfoBelswiss info = new SMSInfoBelswiss("zzz");
            string str = info.CardNumber;

        }
    }
}
