using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using Naviam.WebUI;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    public class AccountsDataAdapter
    {
        private const string CacheKey = "companyAcc";

        public static List<Account> GetAccounts(int? companyId) { return GetAccounts(companyId, null, false); }
        public static List<Account> GetAccounts(int? companyId, int? languageId, bool forceSqlLoad)
        {
            List<Account> res = CacheWrapper.GetList<Account>(CacheKey, companyId, languageId);
            if (res == null || forceSqlLoad)
            {
                res = new List<Account>();
                using (SqlConnectionHolder holder = SqlConnectionHelper.GetConnection(SqlConnectionHelper.ConnectionType.Naviam))
                {
                    using (SqlCommand cmd = holder.Connection.CreateSPCommand("accounts_get"))
                    {
                        cmd.Parameters.AddWithValue("@id_company", companyId);
                        cmd.Parameters.AddWithValue("@id_language", languageId.ToDbValue());
                        try
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    res.Add(new Account(reader));
                            }
                        }
                        catch (SqlException e)
                        {
                            throw e;
                        }
                    }
                }
                //save to cache
                CacheWrapper.SetList<Account>(CacheKey, res, companyId, languageId);
            }
            return res;
        }

        public static Account GetAccount(int? id, int? companyId) { return GetAccount(id, companyId, null, false); }
        public static Account GetAccount(int? id, int? companyId, int? languageId, bool forceSqlLoad)
        {
            Account res = CacheWrapper.GetFromList<Account>(CacheKey, new Account() { Id = id }, companyId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                //TODO: check that trans belongs to company
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
                            throw e;
                        }
                    }
                }
                //save to cache
                if (res == null) // not found in cache->add
                    CacheWrapper.AddToList<Account>(CacheKey, res, companyId);
                else
                    CacheWrapper.UpdateList<Account>(CacheKey, res, companyId);
            }
            return res;
        }

        private static int InsertUpdate(Account entity, int? companyId, int? languageId, DbActionType action)
        {
            int res = -1;
            using (SqlConnectionHolder holder = SqlConnectionHelper.GetConnection(SqlConnectionHelper.ConnectionType.Naviam))
            {
                string commName = action == DbActionType.Insert ? "account_create" : "account_update";
                SqlCommand command = holder.Connection.CreateSPCommand(commName);
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
                    throw e;
                }
            }
            if (res == 0)
            {
                if (action == DbActionType.Insert)
                    CacheWrapper.AddToList<Account>(CacheKey, entity, companyId, languageId);
                if (action == DbActionType.Update)
                    //if ok - update cache
                    CacheWrapper.UpdateList<Account>(CacheKey, entity, companyId, languageId);
            }
            return res;
        }

        public static int Insert(Account entity, int? companyId, int? languageId)
        {
            return InsertUpdate(entity, companyId, languageId, DbActionType.Insert);
        }

        public static int Update(Account entity, int? companyId, int? languageId)
        {
            //TODO: check that account belongs to company
            return InsertUpdate(entity, companyId, languageId, DbActionType.Update);
        }

        //we need to provide full object (not only id) to delete from redis (restrict of redis)
        public static int Delete(Account entity, int? companyId, int? languageId)
        {
            int res = -1;
            //TODO: check that account belongs to company
            using (var holder = SqlConnectionHelper.GetConnection(SqlConnectionHelper.ConnectionType.Naviam))
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
                        throw e;
                    }
                }
            }
            if (res == 0)
            {
                //if ok - remove from cache
                CacheWrapper.RemoveFromList<Account>(CacheKey, entity, companyId, languageId);
            }
            return res;
        }

    }
}
