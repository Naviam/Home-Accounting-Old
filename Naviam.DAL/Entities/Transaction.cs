using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using System.Data.SqlClient;

namespace Naviam.Data
{
    public enum TransactionTypes { Manual = 0, SMS, Statement }
    public enum TransactionDirections { Expense = 0, Income }

    [Serializable]
    public class TransactionsSplit : DbEntity
    {
        public List<Transaction> Items { get; set; }

        public decimal? EndAmount { get; set; }
    }

    [Serializable]
    public class Transaction : DbEntity
    {
        public const string TAG_SEPARATOR = ",";

        public Transaction() 
        {
            //default props for add
            Amount = 0;
            IncludeInTax = false;
            TagIds = new List<string>();
        }
        public Transaction(SqlDataReader reader) : this()
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
            CategoryId = reader["id_category"] as int?;
            CurrencyId = reader["id_currency"] as int?;
            IncludeInTax = reader["include_in_tax"] as bool?;
            string tags = reader["tags"] as string;
            if (!string.IsNullOrEmpty(tags)) TagIds = tags.Split(new string[] { TAG_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public DateTime? Date { get; set; }
        public string Description { get; set; }
        public int? CategoryId { get; set; }
        public int? CurrencyId { get; set; }
        public decimal? Amount { get; set; }
        public string Merchant { get; set; }
        public string Notes { get; set; }
        public List<string> TagIds { get; set; }
        public TransactionTypes TransactionType { get; set; }
        public TransactionDirections Direction { get; set; }
        public int? AccountId { get; set; }
        public bool? IncludeInTax { get; set; }

        #region ICloneable Members

        public Transaction Clone()
        {
            var res = new Transaction();
            res.Date = Date;
            res.Description = Description;
            res.CategoryId = CategoryId;
            res.CurrencyId = CurrencyId;
            res.Amount = Amount;
            res.Merchant = Merchant;
            res.Notes = Notes;
            res.TagIds = TagIds;
            res.TransactionType = TransactionType;
            res.Direction = Direction;
            res.AccountId = AccountId;
            res.IncludeInTax = IncludeInTax;
            return res;
        }

        #endregion
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
            command.Parameters.Add("@type", SqlDbType.Int).Value = transaction.TransactionType;
            command.Parameters.Add("@direction", SqlDbType.Int).Value = transaction.Direction;
            command.Parameters.Add("@include_in_tax", SqlDbType.Bit).Value = transaction.IncludeInTax;
            command.Parameters.Add("@tags", SqlDbType.NVarChar).Value = string.Join(Transaction.TAG_SEPARATOR, transaction.TagIds);
        }
    }
}
