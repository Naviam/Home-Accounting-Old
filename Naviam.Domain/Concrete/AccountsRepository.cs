using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using Naviam.DAL;

namespace Naviam.Domain.Concrete
{
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

        public static int Insert(Account entity, int? companyId) { return Insert(entity, companyId, true); }
        public static int Insert(Account entity, int? companyId, bool intoCache)
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
            var res = AccountsDataAdapter.Delete(entity, companyId);
            if (res == 0)
            {
                //if ok - remove from cache
                new CacheWrapper().RemoveFromList(CacheKey, entity, companyId);
            }
            return res;
        }

        public virtual int ChangeBalance(int? accountId, int? companyId, decimal value)
        {
            var res = AccountsDataAdapter.ChangeBalance(accountId, companyId, value);
            var cache = new CacheWrapper();
            if (res == 0)
            {
                Account account = cache.GetFromList(CacheKey, new Account() { Id = accountId }, companyId);
                account.Balance = account.Balance + value;
                cache.UpdateList(CacheKey, account, companyId);
            }
            return res;
        }

        public virtual Account GetAccountBySms(string cardNumber, int? id_modem, int? id_bank) { return GetAccountBySms(cardNumber, id_modem, id_bank, false); }
        public virtual Account GetAccountBySms(string cardNumber, int? id_modem, int? id_bank, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            Account res = null;
            //load from DB
            res = SmsDataAdapter.GetAccountBySms(cardNumber, id_modem, id_bank);
            var res2 = cache.GetFromList(CacheKey, new Account() { Id = res.Id }, res.CompanyId);
            if (res != null)
            {
                //save to cache
                if (res2 == null) // not found in cache->add
                    cache.AddToList<Account>(CacheKey, res, res.CompanyId);
                else
                    cache.UpdateList(CacheKey, res, res.CompanyId);
            }
            return res;
        }
    }//AccountsRepository
}
