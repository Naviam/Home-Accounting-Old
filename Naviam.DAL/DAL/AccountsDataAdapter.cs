using System.Collections.Generic;
using Naviam.Data;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    
    public class AccountsDataAdapter
    {
        
        public static List<Account> GetAccounts(params object[] id)
        {
            List<Account> res = new List<Account>();
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("accounts_get"))
                {
                    cmd.Parameters.AddWithValue("@id_company", ((int?)id[0]).ToDbValue());
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                res.Add(new Account(reader));
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
        //public static List<Account> GetAccounts(int? companyId)
        //{
        //    List<Account> res = new List<Account>();
        //    using (var holder = SqlConnectionHelper.GetConnection())
        //    {
        //        using (var cmd = holder.Connection.CreateSPCommand("accounts_get"))
        //        {
        //            cmd.Parameters.AddWithValue("@id_company", companyId.ToDbValue());
        //            try
        //            {
        //                using (var reader = cmd.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                        res.Add(new Account(reader));
        //                }
        //            }
        //            catch (SqlException e)
        //            {
        //                cmd.AddDetailsToException(e);
        //                throw;
        //            }
        //        }
        //    }
        //    return res;
        //}

        public static Account GetAccount(int? id, int? companyId)
        {
            Account res = null;
            //TODO: check that account belongs to company
            using (SqlConnectionHolder holder = SqlConnectionHelper.GetConnection(SqlConnectionHelper.ConnectionType.Naviam))
            {
                using (SqlCommand cmd = holder.Connection.CreateSPCommand("accounts_get"))
                {
                    cmd.Parameters.AddWithValue("@id_account", id);
                    try
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            res = new Account(reader);
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

        public static int InsertUpdate(Account entity, int? companyId, DbActionType action)
        {
            var res = -1;
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                var commName = action == DbActionType.Insert ? "account_create" : "account_update";
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

        public static int Delete(Account entity, int? companyId)
        {
            var res = -1;
            //TODO: check that account belongs to company
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var command = holder.Connection.CreateSPCommand("accounts_delete"))
                {
                    try
                    {
                        command.AddCommonParameters(entity.Id);
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
