using System.Collections.Generic;
using Naviam.Data;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    
    public class AccountsDataAdapter
    {

        //public static List<Account> GetAccounts(Dictionary<string, object> parameters)
        //{
        //    List<Account> res = new List<Account>();
        //    using (var holder = SqlConnectionHelper.GetConnection())
        //    {
        //        using (var cmd = holder.Connection.CreateSPCommand("accounts_get"))
        //        {
        //            foreach( var param in parameters)
        //            {
        //                cmd.Parameters.AddWithValue(param.Key, param.Value);
        //            }
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

        public static List<Account> GetAccounts(int? companyId)
        {
            List<Account> res = new List<Account>();
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("accounts_get"))
                {
                    cmd.Parameters.AddWithValue("@id_company", companyId.ToDbValue());
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

        public static Account GetAccount(int? id, int? companyId)
        {
            Account res = null;
            //TODO: check that account belongs to company
            using (SqlConnectionHolder holder = SqlConnectionHelper.GetConnection(SqlConnectionHelper.ConnectionType.Naviam))
            {
                using (SqlCommand cmd = holder.Connection.CreateSPCommand("accounts_get"))
                {
                    cmd.Parameters.AddWithValue("@id_account", id);
                    cmd.Parameters.AddWithValue("@id_company", companyId.ToDbValue());
                    try
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            if (reader.HasRows)
                            {
                                res = new Account(reader);
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
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var command = holder.Connection.CreateSPCommand("accounts_delete"))
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
