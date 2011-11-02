using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace Naviam.Data
{
    public class Budget: DbEntity
    {
        public Budget()
        {

        }

        public Budget(SqlDataReader reader)
        {
            Id = reader["id"] as int?;
            CategoryId = reader["id_category"] as int?;
            Amount = reader["amount"] as decimal?;
            BudgetDate = reader["budget_date"] as DateTime?;
            CompanyId = reader["id_company"] as int?;
            PeriodType = reader["period_type"] as int?;
        }

        public int? CategoryId { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? BudgetDate { get; set; }
        public int? CompanyId { get; set; }
        public int? PeriodType { get; set; }

    }

    public static partial class SqlCommandExtensions
    {
        /// <summary>
        /// Appends Account-specific parameters to the specificied SqlCommand. 
        /// </summary>
        /// <param name="command">SqlCommand to be executed.</param>
        /// <param name="alert">Instance of Budget class</param>
        /// <param name="action">Database action type (select, insert, update, delete).</param>
        public static void AddEntityParameters(this SqlCommand command, Budget budget, DbActionType action)
        {
            command.AddCommonParameters(budget.Id, action);
            command.Parameters.Add("@id_category", SqlDbType.Int).Value = budget.CategoryId;
            command.Parameters.Add("@amount", SqlDbType.Money).Value = budget.Amount;
            command.Parameters.Add("@budget_date", SqlDbType.DateTime).Value = budget.BudgetDate.ToDbValue();
            command.Parameters.Add("@id_company", SqlDbType.Int).Value = budget.CompanyId.ToDbValue();
            command.Parameters.Add("@period_type", SqlDbType.SmallInt).Value = budget.PeriodType.ToDbValue();
        }
    }
}
