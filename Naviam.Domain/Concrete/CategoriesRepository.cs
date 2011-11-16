using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using Naviam.DAL;
using System.Data.SqlClient;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;

namespace Naviam.Domain.Concrete
{
    public class CategoriesRepository
    {
        private const string CacheKey = "transCategory";
        private const string MerchCacheKey = "transMerchCategory";
        private const int DEFAULT_CATEGORY_ID = 20; // 20 - Uncategorized

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
            //Localize
            var rm = new ResourceManager(typeof(Resources.Dicts));
            foreach (var item in res)
            {
                var st = rm.GetString("c_" + item.Id.ToString());
                if (!String.IsNullOrEmpty(st))
                    item.Name = st;
            }
            //end Localize
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

        public virtual List<CategoryMerchant> GetMerchantsCategories() { return GetMerchantsCategories(false); }
        public virtual List<CategoryMerchant> GetMerchantsCategories(bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<CategoryMerchant>(MerchCacheKey);

            if (res == null || forceSqlLoad)
            {
                //load from DB
                res = CategoriesDataAdapter.GetCategoriesMerchant();
                //save to cache
                cache.SetList(MerchCacheKey, res);
            }
            //end Localize
            return res;
        }

        public virtual int? FindCategoryMerchant(int? idCompany, string merchant)
        {
            //get category by user rules
            RulesRepository rep = new RulesRepository();
            string val = rep.GetValueByRules("merchant", merchant, "id_category", idCompany);
            int ires = 0;
            if (int.TryParse(val, out ires))
                return ires;

            //get by statistic from db
            var stats = GetMerchantsCategories();
            var res = stats.FirstOrDefault(x => x.Merchant.Equals(merchant, StringComparison.InvariantCultureIgnoreCase));
            return res!=null ? res.CategoryId.Value : DEFAULT_CATEGORY_ID;
        }

        public delegate List<CategoryMerchant> GetMerchantsCategoriesAsynchCaller();

        public List<CategoryMerchant> GetMerchantsCategoriesAsynch()
        {
            //Thread.Sleep(10000);
            return GetMerchantsCategories(true);
        }
    }
}
