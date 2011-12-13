using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace Naviam.Data
{
    public class BelSwissSms : SmsBase
    {
        public BelSwissSms(string sms)
            : base(sms)
        { }

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
            string pattern = @"(?<operation>Retail|Service payment from card|Service payment to card|ATM|Cash)";
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
            string pattern = @"([^\x00]*(?<operation>Retail|Service payment from card|Service payment to card|ATM|Cash)(?<result>[^\x00]*?)(?<date>(?:\d{4}|\d{2})-\d{1,2}-\d{1,2}\s\d{1,2}:\d{1,2}:\d{1,5})[^\x00]*)";
            Regex reg = new Regex(pattern, RegexOptions.Multiline | RegexOptions.CultureInvariant);
            if (reg.IsMatch(_sms))
            {
                Match match = reg.Match(_sms);
                result = match.Groups["result"].Success ? match.Groups["result"].Value.Trim() : "";
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
            string pattern = @"Na vremya:\s\d{1,2}:\d{1,2}:\d{1,5}\r?\n?(?<merchant>[^\x00]*)";
            Regex reg = new Regex(pattern, RegexOptions.Multiline | RegexOptions.CultureInvariant);
            if (reg.IsMatch(_sms))
            {
                Match match = reg.Match(_sms);
                result = match.Groups["merchant"].Success ? match.Groups["merchant"].Value.Trim() : "";
            }
            return result;
        }

        public override TransactionDirections GetDirection()
        {
            TransactionDirections res = TransactionDirections.Expense;
            if (Operation == "Service payment to card")
                res = TransactionDirections.Income;
            return res;
        }

        public override string GetHtmlText()
        {
            string strSms = _sms;
            string ptrn = @"([^\x00]*(?<operation>Retail|Service payment from card|Service payment to card|ATM|Cash)(?<result>[^\x00]*?)(?<date>(?:\d{4}|\d{2})-\d{1,2}-\d{1,2}\s\d{1,2}:\d{1,2}:\d{1,5})[^\x00]*)";
            Regex reg = new Regex(ptrn, RegexOptions.Multiline | RegexOptions.CultureInvariant);
            string strRes = string.Empty;
            Match match;
            if (reg.IsMatch(_sms))
            {
                match = reg.Match(_sms);
                strRes = match.Groups["result"].Success ? match.Groups["result"].Value : "";
            }
            string colorStr = "Red";
            if (strRes.Trim().Equals("Uspeshno", StringComparison.InvariantCultureIgnoreCase))
                colorStr = "Green";
            strSms = strSms.Replace(strRes, string.Format("\r\n<span style='font-weight:bold;color:{1}'>{0}</span>\r\n", strRes.Trim(), colorStr));
            
            ptrn = @"Summa:\s?(?<amount>[^\x00]*?)\s";
            strSms = Regex.Replace(strSms, ptrn, "Summa: <span style='font-weight:bold;'>${amount}</span> ");
            ptrn = @"Ostatok:\s?(?<ost>[^\x00]*?)\s";
            strSms = Regex.Replace(strSms, ptrn, "Ostatok: <span style='font-weight:bold;'>${ost}</span> ");
            string res = string.Empty;
            foreach (string line in strSms.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                res = res + "<tr><td>" + line + "</td></tr>";
            }
            res = "<table>" + res + "</table>";
            return res;
        }

        public override string GetText()
        {
            return _sms;
        }
    }
}