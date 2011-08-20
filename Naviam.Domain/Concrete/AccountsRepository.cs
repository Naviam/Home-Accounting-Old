using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using Naviam.DAL;

namespace Naviam.Domain.Concrete
{
   
    public class Repository<T> where T: class
    {
        public delegate List<T> GetListDelegat<T>(Dictionary<string, object> parameters);

        public static string CacheKey = typeof(T).ToString();
        
        public virtual void ResetCache(params int?[] id)
        {
            
            var cache = new CacheWrapper();
            cache.SetList<T>(CacheKey, null, id);
        }

        public static List<T> GetList(GetListDelegat<T> dlgt, Dictionary<string, object> searchParams, params int?[] id) { return GetList(dlgt, searchParams, false, id); }
        public static List<T> GetList(GetListDelegat<T> dlgt, Dictionary<string, object> searchParams, bool forceSqlLoad, params int?[] id)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<T>(CacheKey, id);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                res = dlgt.Invoke(searchParams);
                //save to cache
                cache.SetList(CacheKey, res, id);
            }
            return res;
        }
    }

    public class AccountsRepository
    {
        private const string CacheKey = "companyAcc";

        public void ResetCache(int? companyId)
        {
            var cache = new CacheWrapper();
            cache.SetList<Account>(CacheKey, null, companyId);
        }

        public static List<Account> GetAccounts(int? companyId) { return GetAccounts(companyId, false); }
        public static List<Account> GetAccounts(int? companyId, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<Account>(CacheKey, companyId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                res = AccountsDataAdapter.GetAccounts(companyId);
                //save to cache
                cache.SetList(CacheKey, res, companyId);
            }
            return res;
        }

        public static Account GetAccount(int? id, int? companyId) { return GetAccount(id, companyId, false); }
        public static Account GetAccount(int? id, int? companyId, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetFromList(CacheKey, new Account() { Id = id }, companyId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                //TODO: check that trans belongs to company
                res = AccountsDataAdapter.GetAccount(id, companyId);
                //save to cache
                if (res == null) // not found in cache->add
                    cache.AddToList<Account>(CacheKey, res, companyId);
                else
                    cache.UpdateList(CacheKey, res, companyId);
            }
            return res;
        }

        private static int InsertUpdate(Account entity, int? companyId, DbActionType action, bool intoCache)
        {
            var cache = new CacheWrapper();
            var res = AccountsDataAdapter.InsertUpdate(entity, companyId, action);
            if (res == 0)
            {
                //if ok - update cache
                if (intoCache)
                {
                    if (action == DbActionType.Insert)
                        cache.AddToList(CacheKey, entity, companyId);
                    if (action == DbActionType.Update)
                        cache.UpdateList(CacheKey, entity, companyId);
                }
            }
            return res;
        }

        public int Insert(Account entity, int? companyId) { return Insert(entity, companyId, true); }
        public int Insert(Account entity, int? companyId, bool intoCache)
        {
            return InsertUpdate(entity, companyId, DbActionType.Insert, intoCache);
        }

        public static int Update(Account entity, int? companyId)
        {
            //TODO: check that account belongs to company
            return InsertUpdate(entity, companyId, DbActionType.Update, true);
        }

        //we need to provide full object (not only id) to delete from redis (restrict of redis)
        public static int Delete(Account entity, int? companyId)
        {
            var res = AccountsDataAdapter.Delete(entity,companyId);
            if (res == 0)
            {
                //if ok - remove from cache
                new CacheWrapper().RemoveFromList(CacheKey, entity, companyId);
            }
            return res;
        }
    }//AccountsRepository
}
