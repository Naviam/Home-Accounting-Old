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

        public static FieldRule GetRule(int? id, int? companyId)
        {
            FieldRule res = null;
            using (SqlConnectionHolder holder = SqlConnectionHelper.GetConnection(SqlConnectionHelper.ConnectionType.Naviam))
            {
                using (SqlCommand cmd = holder.Connection.CreateSPCommand("user_rules_get"))
                {
                    cmd.Parameters.AddWithValue("@id_company", companyId.ToDbValue());
                    cmd.Parameters.AddWithValue("@id_rule", id);
                    try
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            if (reader.HasRows)
                            {
                                res = new FieldRule(reader);
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

        public static int InsertUpdate(FieldRule entity, int? companyId, DbActionType action)
        {
            var res = -1;
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                var commName = action == DbActionType.Insert ? "user_rule_create" : "user_rule_update";
                var command = holder.Connection.CreateSPCommand(commName);
                try
                {
                    command.AddEntityParameters(entity, action);
                    command.ExecuteNonQuery();
                    if (action == DbActionType.Insert)
                        entity.Id = command.GetRowIdParameter();
                    res = command.GetReturnParameter();
                }
                catch (SqlException e)
                {
                    command.AddDetailsToException(e);
                    throw;
                }
            }
            return res;
        }

        public static int Delete(FieldRule entity, int? companyId)
        {
            var res = -1;
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var command = holder.Connection.CreateSPCommand("user_rule_delete"))
                {
                    try
                    {
                        command.AddCommonParameters(entity.Id);
                        command.Parameters.AddWithValue("@id_company", companyId.ToDbValue());
                        command.ExecuteNonQuery();
                        res = command.GetReturnParameter();
                    }
                    catch (SqlException e)
                    {
                        command.AddDetailsToException(e);
                        throw;
                    }
                }
            }
            return res;
        }
    }
}
