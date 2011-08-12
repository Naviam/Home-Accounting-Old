using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Naviam.WebUI.Helpers.Parsers
{
    public class ParserStatement
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string Account { get; set; }
        public string Name { get; set; }
        public string BankName { get; set; }
        public List<ParserTransaction> Transactions { get; set; }

        public ParserStatement()
        {
            Transactions = new List<ParserTransaction>();
        }
    }

    public class ParserTransaction
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

    public abstract class ParserBase
    {
        public abstract bool IsMyFile(string fileName);
        public abstract ParserStatement ParseFile(string fileName);
    }
}