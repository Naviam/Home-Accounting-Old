using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace Naviam.WebUI.Helpers.Sms
{
    public class BelSwissSms : SmsBase
    {
        public BelSwissSms(string sms)
            : base(sms)
        {

        }

        public override string GetCardNumber()
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

        public override string GetOperation()
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

        public override string GetResult()
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

        public override DateTime GetDate()
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

        public override decimal GetAmount()
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

        public override string GetShortCurrency()
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

        public override string GetMerchant()
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