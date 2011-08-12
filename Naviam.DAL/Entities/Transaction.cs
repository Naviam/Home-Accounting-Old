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
        public enum TransactionDirections { Expence  = 0, Income }

        public Transaction() { }
        public Transaction(SqlDataReader reader) 
        { 
            Id = reader["id"] as int?; 
            Date = reader["date"] as DateTime?;
            Amount = reader["amount"] as decimal?;
            Merchant = reader["merchant"] as string;
            Description = reader["description"] as string;
            Notes = reader["notes"] as string;
		    TransactionType = (TransactionTypes)reader["transaction_type"];
            Direction = (TransactionDirections)reader["direction"];
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
            command.Parameters.Add("@merchant", SqlDbType.NVarChar).Value = transaction.Merchant;
            command.Parameters.Add("@id_account", SqlDbType.Int).Value = transaction.AccountId;
            command.Parameters.Add("@id_category", SqlDbType.Int).Value = transaction.CategoryId;
            command.Parameters.Add("@description", SqlDbType.NVarChar).Value = transaction.Merchant;
            command.Parameters.Add("@notes", SqlDbType.NVarChar).Value = transaction.Notes;
            command.Parameters.Add("@type", SqlDbType.Int).Value = transaction.TransactionType;
            command.Parameters.Add("@direction", SqlDbType.Int).Value = transaction.Direction;
        }
    }
}
