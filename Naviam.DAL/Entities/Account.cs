using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace Naviam.Data
{
    [Serializable]
    public class Account : DbEntity
    {
        public Account() { }
        public Account(SqlDataReader reader)
        {
            Id = reader["id"] as int?;
            Number = reader["number"] as string;
            DateCreation = reader["date_creation"] as DateTime?;
            InitialBalance = reader["balance"] as decimal?;
            Description = reader["description"] as string;
            CompanyId = reader["id_company"] as int?;
            CurrencyId = reader["id_currency"] as int?;
            Currency = reader["currency"] as string;
            TypeId = reader["id_type"] as int?;
            TypeName = reader["type_name"] as string;
            //TODO: calculate Balance
            Balance = 0;
        }

        public string Number { get; set; }
        public DateTime? DateCreation { get; set; }
        public decimal? InitialBalance { get; set; }
        public decimal? Balance { get; set; }
        public string Description { get; set; }
        public int? CompanyId { get; set; }
        public int? CurrencyId { get; set; }
        public string Currency { get; set; }
        public int? TypeId { get; set; }
        public string TypeName { get; set; }
    }

    public static partial class SqlCommandExtensions
    {
        /// <summary>
        /// Appends Account-specific parameters to the specificied SqlCommand. 
        /// </summary>
        /// <param name="command">SqlCommand to be executed.</param>
        /// <param name="alert">Instance of Account class</param>
        /// <param name="action">Database action type (select, insert, update, delete).</param>
        public static void AddEntityParameters(this SqlCommand command, Account account, DbActionType action)
        {
            command.AddCommonParameters(account.Id, action);
            command.Parameters.Add("@curr_date_utc", SqlDbType.DateTime).Value = DateTime.UtcNow;
            command.Parameters.Add("@number", SqlDbType.NVarChar).Value = account.Number.ToDbValue();
            command.Parameters.Add("@id_company", SqlDbType.Int).Value = account.CompanyId.ToDbValue();
            command.Parameters.Add("@id_currency", SqlDbType.Int).Value = account.CurrencyId.ToDbValue();
            command.Parameters.Add("@balance", SqlDbType.Decimal).Value = account.InitialBalance;
            command.Parameters.Add("@id_type", SqlDbType.Int).Value = account.TypeId.ToDbValue();
            command.Parameters.Add("@description", SqlDbType.NVarChar).Value = account.Description.ToDbValue();
        }
    }
}
