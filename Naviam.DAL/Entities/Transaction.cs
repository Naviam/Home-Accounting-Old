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
            CategoryId = reader["category_id"] as int?;
            Category = reader["category_name"] as string; 
        }

        public DateTime? Date { get; set; }

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

        public static string GetTransactionTypeString(TransactionTypes tranType)
        {
            switch (tranType)
            {
                case TransactionTypes.Cash: return Cash;
                case TransactionTypes.Check: return Check;
                case TransactionTypes.Pending: return Pending;
                default: return Cash;
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

        public static string GetTransactionDirectionString(TransactionDirections tranDirection)
        {
            switch (tranDirection)
            {
                case TransactionDirections.Income: return Income;
                case TransactionDirections.Expence: return Expence;
                default: return Income;
            }
        }
    }

    public static partial class SqlCommandExtensions
    {
        /// <summary>
        /// Appends Transaction-specific parameters to the specificied SqlCommand. 
        /// </summary>
        /// <param name="command">SqlCommand to be executed.</param>
        /// <param name="alert">Instance of Transaction class</param>
        /// <param name="action">Database action type (select, insert, update, delete).</param>
        public static void AddEntityParameters(this SqlCommand command, Transaction transaction, DbActionType action)
        {
            command.AddCommonParameters(transaction.Id, action);
            command.Parameters.Add("@date", SqlDbType.DateTime).Value = transaction.Date;
            command.Parameters.Add("@amount", SqlDbType.Decimal).Value = transaction.Amount;
            command.Parameters.Add("@merchant", SqlDbType.NVarChar).Value = transaction.Merchant.ToDbValue();
            command.Parameters.Add("@id_account", SqlDbType.Int).Value = transaction.AccountId.ToDbValue();
            command.Parameters.Add("@id_category", SqlDbType.Int).Value = transaction.CategoryId.ToDbValue();
            command.Parameters.Add("@description", SqlDbType.NVarChar).Value = transaction.Description.ToDbValue();
            command.Parameters.Add("@notes", SqlDbType.NVarChar).Value = transaction.Notes.ToDbValue();
            command.Parameters.Add("@type", SqlDbType.NVarChar).Value = Transaction.GetTransactionTypeString(transaction.TransactionType);
            command.Parameters.Add("@direction", SqlDbType.NVarChar).Value = Transaction.GetTransactionDirectionString(transaction.Direction);
        }
    }
}
