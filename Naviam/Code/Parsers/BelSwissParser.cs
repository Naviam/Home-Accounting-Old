using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Naviam.Code.Parsers
{
    public class BelSwissStatement
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string Account { get; set; }
        public string Name { get; set; }
        public List<BelSwissTransaction> Transactions { get; set; }

        public BelSwissStatement()
        {
            Transactions = new List<BelSwissTransaction>();
        }
    }

    public class BelSwissTransaction
    {
        public DateTime OperationDate { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Card { get; set; }
        public string OperationDescription { get; set; }
        public string Region { get; set; }
        public string Place { get; set; }
        public string Currency { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal AccountAmount { get; set; }
        public decimal Balance { get; set; }
    }

    public class BelSwissParser
    {
        const string HEADER_PATTERN = @"Subject: Выписка по счету\s*(?<account>[^\s]+) за период с (?<startDate>[^\s]+) по (?<endDate>[^\s]+)\s*(?<client>.*)";
        const string TABLE_PATTERN = @"<table[^>]*=sea[^>]*>(?<content>.*?)</table>";
        const string ROW_PATTERN = @"<tr[^>]*>(?<content>.*?)</tr>";
        const string TD_PATTERN = @"<td[^>]*>(?<content>.*?)</td>";
       
        public bool IsMyFile(string fileName)
        {
            bool res = false;
            string content = GetContent(fileName);
            Match def = Regex.Match(content, HEADER_PATTERN, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (def.Success && content.Contains("ЗАО \"БелСвиссБанк\""))
                res = true;
            return res;
        }

        public BelSwissStatement ParseFile(string fileName)
        {
            BelSwissStatement res = null;
            string content = GetContent(fileName);
            content = content.Replace("&nbsp;", "");
            CultureInfo ruCulture = new CultureInfo("ru", false);
            Match header = Regex.Match(content, HEADER_PATTERN, RegexOptions.IgnoreCase  | RegexOptions.Multiline);
            if (header.Success)
            {
                Match table = Regex.Match(content, TABLE_PATTERN, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
                if (table.Success)
                {
                    DateTime opDate;
                    res = new BelSwissStatement();
                    if (!DateTime.TryParse(header.Groups["startDate"].Value.Trim(), ruCulture.DateTimeFormat, DateTimeStyles.None, out opDate))
                        throw new FormatException();
                    res.From = opDate;
                    if (!DateTime.TryParse(header.Groups["endDate"].Value.Trim(), ruCulture.DateTimeFormat, DateTimeStyles.None, out opDate))
                        throw new FormatException();
                    res.To = opDate;
                    res.Account = header.Groups["account"].Value.Trim();
                    res.Name = header.Groups["client"].Value.Trim();

                    content = table.Groups["content"].Value.Trim();
                    foreach (Match row in Regex.Matches(content, ROW_PATTERN, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline))
                    {
                        string srow = row.Groups["content"].Value.Trim();
                        bool isTransaction = false;
                        int nColumn = 1;
                        BelSwissTransaction trans = null;
                        foreach (Match col in Regex.Matches(srow, TD_PATTERN, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline))
                        {
                            string scol = col.Groups["content"].Value.Trim();
                            decimal amount;
                            if (nColumn == 1 && DateTime.TryParse(scol, ruCulture.DateTimeFormat, DateTimeStyles.None, out opDate))
                            {
                                //found transasction
                                isTransaction = true;
                                trans = new BelSwissTransaction();
                                trans.OperationDate = opDate;
                            }
                            if (isTransaction && nColumn == 2)
                            {
                                if (DateTime.TryParse(scol, ruCulture.DateTimeFormat, DateTimeStyles.None, out opDate))
                                    trans.TransactionDate = opDate;
                                else
                                {
                                    //error
                                    throw new FormatException();
                                }
                            }
                            if (isTransaction && nColumn == 3)
                                trans.Card = scol;
                            if (isTransaction && nColumn == 4)
                                trans.OperationDescription = scol;
                            if (isTransaction && nColumn == 5)
                                trans.Region = scol;
                            if (isTransaction && nColumn == 6)
                                trans.Place = scol;
                            if (isTransaction && nColumn == 7)
                                trans.Currency = scol;
                            if (isTransaction && nColumn == 8)
                            {
                                if (Decimal.TryParse(scol, out amount))
                                    trans.TransactionAmount = amount;
                                else
                                {
                                    //error
                                    throw new FormatException();
                                }
                            }
                            if (isTransaction && nColumn == 9)
                            {
                                if (Decimal.TryParse(scol, out amount))
                                    trans.AccountAmount = amount;
                                else
                                {
                                    //error
                                    throw new FormatException();
                                }
                            }
                            if (isTransaction && nColumn == 10)
                            {
                                if (Decimal.TryParse(scol, out amount))
                                    trans.Balance = amount;
                                else
                                {
                                    //error
                                    throw new FormatException();
                                }
                            }
                            nColumn++;
                        }
                        if (isTransaction)
                            res.Transactions.Add(trans);
                    }
                }
            }
            return res;
        }

        public string GetContent(string fileName)
        {
            string content = null;
            using (TextReader sr = new StreamReader(fileName, System.Text.Encoding.GetEncoding(1251)))
            {
                content = sr.ReadToEnd();
            }
            return content;
        }
    }
}