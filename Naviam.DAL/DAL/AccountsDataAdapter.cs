using System.Collections.Generic;
using Naviam.Data;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    public class AccountsDataAdapter
    {
        private const string CacheKey = "companyAcc";

        public static List<Account> GetAccounts(int? companyId) { return GetAccounts(companyId, null, false); }
        public static List<Account> GetAccounts(int? companyId, int? languageId, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<Account>(CacheKey, companyId, languageId);
            if (res == null || forceSqlLoad)
            {
                res = new List<Account>();
                using (var holder = SqlConnectionHelper.GetConnection())
                {
                    using (var cmd = holder.Connection.CreateSPCommand("accounts_get"))
                    {
                        cmd.Parameters.AddWithValue("@id_company", companyId);
                        cmd.Parameters.AddWithValue("@id_language", languageId.ToDbValue());
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
                //save to cache
                cache.SetList(CacheKey, res, companyId, languageId);
            }
            return res;
        }

        public static Account GetAccount(int? id, int? companyId) { return GetAccount(id, companyId, null, false); }
        public static Account GetAccount(int? id, int? companyId, int? languageId, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetFromList(CacheKey, new Account() { Id = id }, companyId);
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
                            cmd.AddDetailsToException(e);
                            throw;
                        }
                    }
                }
                //save to cache
                if (res == null) // not found in cache->add
                    cache.AddToList<Account>(CacheKey, res, companyId);
                else
                    cache.UpdateList(CacheKey, res, companyId);
            }
            return res;
        }

        private static int InsertUpdate(Account entity, int? companyId, int? languageId, DbActionType action)
        {
            var cache = new CacheWrapper();
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
            if (res == 0)
            {
                if (action == DbActionType.Insert)
                    cache.AddToList(CacheKey, entity, companyId, languageId);
                if (action == DbActionType.Update)
                    //if ok - update cache
                    cache.UpdateList(CacheKey, entity, companyId, languageId);
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
            var cache = new CacheWrapper();
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
            if (res == 0)
            {
                //if ok - remove from cache
                cache.RemoveFromList(CacheKey, entity, companyId, languageId);
            }
            return res;
        }

    }
}
