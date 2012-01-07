using System;

namespace Naviam.InternetBank.Entities
{
    public class AccountTransaction
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
        public string Status { get; set; }
    }
}
