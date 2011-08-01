using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using System.Data.SqlClient;

namespace Naviam.Data
{
    [Serializable]
    public class Transaction : DbEntity
    {
        public enum TransactionTypes { Cash = 0, Check, Pending }
        const string Cash = "cash";
        const string Check = "check";
        const string Pending = "pending";

        public enum TransactionDirections { Income = 0, Expence }
        const string Income = "income";
        const string Expence = "expence";


        public Transaction() { }
        public Transaction(SqlDataReader reader) 
        { 
            Id = reader["id"] as int?; 
            Date = reader["date"] as DateTime?;
            Amount = reader["amount"] as decimal?;
            Merchant = reader["merchant"] as string;
            Description = reader["description"] as string;
            Notes = reader["notes"] as string;
		    TransactionType = GetTransactionType(reader["transaction_type"] as string);
            Direction = GetTransactionDirection(reader["direction"] as string);
            AccountId = reader["id_account"] as int?; 
		    AccountNumber = reader["account_number"] as string;
            AccountType = Account.GetAccountType(reader["account_type"] as string);
            Category = reader["category_name"] as string; 
        }

        public DateTime? Date { get; set; }
        public string FormattedDate
        {
            get
            {
                if (Date.HasValue)
                    return Date.Value.ToString(Thread.CurrentThread.CurrentUICulture);
                return null;
            }
        }
        public string Description { get; set; }
        public string Category { get; set; }
        public int? CategoryId { get; set; }
        public decimal? Amount { get; set; }
        public string Merchant { get; set; }
        public string Notes { get; set; }
        public TransactionTypes TransactionType { get; set; }
        public TransactionDirections Direction { get; set; }
        public int? AccountId { get; set; }
        public string AccountNumber { get; set; }
        public Account.AccountTypes AccountType { get; set; }

        public static TransactionTypes GetTransactionType(string tranTypeStr)
        {
            switch (tranTypeStr)
            {
                case Cash: return TransactionTypes.Cash;
                case Check: return TransactionTypes.Check;
                case Pending: return TransactionTypes.Pending;
                default: return TransactionTypes.Cash;
            }
        }

        public static TransactionDirections GetTransactionDirection(string tranDirectionStr)
        {
            switch (tranDirectionStr)
            {
                case Income: return TransactionDirections.Income;
                case Expence: return TransactionDirections.Expence;
                default: return TransactionDirections.Income;
            }
        }
    }
}
