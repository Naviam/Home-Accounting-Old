using System;
using System.Collections.Generic;
using Naviam.Data;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    public class RulesDataAdapter
    {
        public static List<FieldRule> GetUserRules(int? companyId)
        {
            var res = new List<FieldRule>();
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("user_rules_get"))
                {
                    cmd.Parameters.AddWithValue("@id_company", companyId);
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                res.Add(new FieldRule(reader));
                            }
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
