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
            cache.SetList<Transaction>(CacheKey, null, userId);
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
    }
}
