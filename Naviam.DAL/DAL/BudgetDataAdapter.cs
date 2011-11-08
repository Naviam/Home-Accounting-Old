using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    public class BudgetDataAdapter
    {
        public static List<Budget> GetBudgets(int? companyId)
        {
            List<Budget> res = new List<Budget>();
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("budgets_get"))
                {
                    cmd.Parameters.AddWithValue("@id_company", companyId.ToDbValue());
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                res.Add(new Budget(reader));
                        }
                    }
                    catch (SqlException e)
                    {
                        cmd.AddDetailsToException(e);
                        throw;
                    }
                }
            }
            return res;
        }
    }
}
