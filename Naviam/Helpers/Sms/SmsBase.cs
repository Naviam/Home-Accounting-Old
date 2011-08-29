using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace Naviam.WebUI.Helpers.Sms
{
    public abstract class SmsBase
    {
        protected string _sms;

        public SmsBase(string sms)
        {
            _sms = sms;
            CardNumber = GetCardNumber();
            Operation = GetOperation();
            Result = GetResult();
            Date = GetDate();
            Amount = GetAmount();
            ShortCurrency = GetShortCurrency();
            Merchant = GetMerchant();
        }

        public string CardNumber { get; set; }
        public string Operation { get; set; }
        public string Result { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string ShortCurrency { get; set; }
        public string Merchant { get; set; }

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
}