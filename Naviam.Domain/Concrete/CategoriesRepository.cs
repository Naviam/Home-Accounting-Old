using System;
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

        public void ResetCache(int? userId)
        {
            var cache = new CacheWrapper();
            cache.SetList<Category>(CacheKey, null, userId);
        }

        public static List<Category> GetCategories(int? userId) { return GetCategories(userId, false); }
        public static List<Category> GetCategories(int? userId, bool forceSqlLoad)
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

        private int Insert(Category entity, int? userId, bool intoCache)
        {
            var cache = new CacheWrapper();
            var res = CategoriesDataAdapter.Insert(entity, userId);
            if (res == 0)
            {
                //if ok - update cache
                if (intoCache)
                {
                    cache.AddToList(CacheKey, entity, userId);
                }
            }
            return res;
        }

        //we need to provide full object (not only id) to delete from redis (restrict of redis)
        public int Delete(Category entity, int? userId)
        {
            var res = CategoriesDataAdapter.Delete(entity, userId);
            if (res == 0)
            {
                //if ok - remove from cache
                new CacheWrapper().RemoveFromList(CacheKey, entity, userId);
            }
            return res;
        }
    }
}
