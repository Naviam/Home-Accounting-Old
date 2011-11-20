using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naviam.Data;
using Naviam.DAL;

namespace Naviam.Domain.Concrete
{
    public class TransactionsRepository
    {
        private const string CacheKey = "userTrans";
        
        public void ResetCache(int? companyId)
        {
            var cache = new CacheWrapper();
            cache.SetList<Transaction>(CacheKey, null, companyId);
        }

        public virtual IEnumerable<Transaction> GetTransactions(int? companyId) { return GetTransactions(companyId, false); }
        public virtual IEnumerable<Transaction> GetTransactions(int? companyId, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<Transaction>(CacheKey, companyId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                res = TransactionsDataAdapter.GetTransactions(companyId);
                //save to cache
                cache.SetList(CacheKey, res, companyId);
            }
            return res;
        }

        public virtual Transaction GetTransaction(int? id, int? companyId) { return GetTransaction(id, companyId, false); }
        public virtual Transaction GetTransaction(int? id, int? companyId, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetFromList(CacheKey, new Transaction { Id = id }, companyId);
            if (res == null || forceSqlLoad)
            {
                bool inCache = res != null;
                //load from DB
                res = TransactionsDataAdapter.GetTransaction(id, companyId);
                //res = new Transaction() { Description = "Test", Category = "Dinner", Amount = 100.20M, Id = 1, Date = DateTime.Now };
                //save to cache
                //if (!inCache) // not found in cache->add
                //    cache.AddToList<Transaction>(CacheKey, res, companyId);
                //else
                //    cache.UpdateList(CacheKey, res, companyId);
            }
            return res;
        }

        public virtual int Delete(int? id, int? companyId)
        {
            var res = TransactionsDataAdapter.Delete(id, companyId);
            if (res == 0)
            {
                //if ok - remove from cache
                new CacheWrapper().RemoveFromList2(CacheKey, new Transaction() { Id = id }, companyId);
            }
            return res;
        }

        private int InsertUpdate(Transaction entity, int? companyId, DbActionType action, bool intoCache)
        {
            var cache = new CacheWrapper();
            var res = TransactionsDataAdapter.InsertUpdate(entity, companyId, action);
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

        public virtual int Insert(Transaction entity, int? companyId) { return Insert(entity, companyId, true); }
        public virtual int Insert(Transaction entity, int? companyId, bool intoCache)
        {
            return InsertUpdate(entity, companyId, DbActionType.Insert, intoCache);
        }

        public virtual int Update(Transaction entity, int? companyId)
        {
            return InsertUpdate(entity, companyId, DbActionType.Update, true);
        }

        public virtual int BatchInsert(List<Transaction> list, int? companyId) { return BatchInsert(list, companyId, true); }
        public virtual int BatchInsert(List<Transaction> list, int? companyId, bool intoCache)
        {
            var cache = new CacheWrapper();
            var res = TransactionsDataAdapter.BatchInsert(list, companyId, DbActionType.Insert);
            if (res == 0)
            {
                //if ok - update cache
                if (intoCache)
                {
                    foreach (var entity in list)
                    {
                        cache.AddToList(CacheKey, entity, companyId);
                    }
                }
            }
            return res;
        }



    }
}
