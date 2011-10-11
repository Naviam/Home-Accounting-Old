using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using Naviam.DAL;

namespace Naviam.Domain.Concrete
{
    public class TagsRepository
    {
        private const string CacheKey = "transactionTags";

        public void ResetCache(int? userId)
        {
            var cache = new CacheWrapper();
            cache.SetList<Tag>(CacheKey, null, userId);
        }

        public virtual List<Tag> GetAll(int? userId) { return GetAll(userId, false); }
        public virtual List<Tag> GetAll(int? userId, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<Tag>(CacheKey, userId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                res = TagsDataAdapter.GetTags(userId);
                //save to cache
                cache.SetList(CacheKey, res, userId);
            }
            return res;
        }

        private int InsertUpdate(Tag entity, int? userId, DbActionType action, bool intoCache)
        {
            var cache = new CacheWrapper();
            var res = TagsDataAdapter.InsertUpdate(entity, userId, action);
            if (res == 0)
            {
                //if ok - update cache
                if (intoCache)
                {
                    if (action == DbActionType.Insert)
                        cache.AddToList(CacheKey, entity, userId);
                    if (action == DbActionType.Update)
                        cache.UpdateList(CacheKey, entity, userId);
                }
            }
            return res;
        }

        public virtual int Insert(Tag entity, int? userId) { return Insert(entity, userId, true); }
        public virtual int Insert(Tag entity, int? userId, bool intoCache)
        {
            return InsertUpdate(entity, userId, DbActionType.Insert, intoCache);
        }

        public virtual int Update(Tag entity, int? userId)
        {
            return InsertUpdate(entity, userId, DbActionType.Update, true);
        }

        public virtual int Delete(int? id, int? userId)
        {
            var res = TagsDataAdapter.Delete(id, userId);
            if (res == 0)
            {
                //if ok - remove from cache
                new CacheWrapper().RemoveFromList2(CacheKey, new Tag() { Id = id }, userId);
            }
            return res;
        }
    }
}
