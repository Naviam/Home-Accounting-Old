﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using Naviam.DAL;
using System.Data.SqlClient;

namespace Naviam.Domain.Concrete
{
    public class CategoriesRepository
    {
        private const string CacheKey = "transCategory";

        public virtual void ResetCache(int? userId)
        {
            var cache = new CacheWrapper();
            cache.SetList<Category>(CacheKey, null, userId);
        }

        public virtual List<Category> GetAll(int? userId) { return GetAll(userId, false); }
        public virtual List<Category> GetAll(int? userId, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<Category>(CacheKey, userId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                res = CategoriesDataAdapter.GetCategories(userId);
                //save to cache
                cache.SetList(CacheKey, res, userId);
            }
            return res;
        }

        private int InsertUpdate(Category entity, int? userId, DbActionType action, bool intoCache)
        {
            var cache = new CacheWrapper();
            var res = CategoriesDataAdapter.InsertUpdate(entity, userId, action);
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

        public virtual int Insert(Category entity, int? userId) { return Insert(entity, userId, true); }
        public virtual int Insert(Category entity, int? userId, bool intoCache)
        {
            return InsertUpdate(entity, userId, DbActionType.Insert, intoCache);
        }

        public virtual int Update(Category entity, int? userId)
        {
            return InsertUpdate(entity, userId, DbActionType.Update, true);
        }

        public virtual int Delete(int? id, int? userId)
        {
            var res = CategoriesDataAdapter.Delete(id, userId);
            if (res == 0)
            {
                //if ok - remove from cache
                new CacheWrapper().RemoveFromList2(CacheKey, new Category() { Id = id }, userId);
            }
            return res;
        }
    }
}
