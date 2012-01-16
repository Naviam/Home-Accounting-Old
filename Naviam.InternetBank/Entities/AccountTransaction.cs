using System;

namespace Naviam.InternetBank.Entities
{
    public class AccountTransaction
    {
        // operation data (card data)
        public DateTime OperationDate { get; set; }
        public string OperationDescription { get; set; }
        public decimal OperationAmount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        // transaction data (account data)
        public DateTime TransactionDate { get; set; }
        public decimal Commission { get; set; }
        public decimal TransactionAmount { get; set; }

        public override string ToString()
        {
            return String.Format("Operation Date: {0}, Description: {1}, Operation Amount: {2}, Currency: {3}, Status: {4}, Transaction Date: {5}, Commission: {6}, Transaction Amount: {7}",
                OperationDate.Date, OperationDescription, OperationAmount, Currency, Status, TransactionDate.Date, Commission, TransactionAmount);
        }

        //public string Card { get; set; }
        //public string Region { get; set; }
        //public string Place { get; set; }
        //public decimal AccountAmount { get; set; }
        //public decimal Balance { get; set; }
    }
}
