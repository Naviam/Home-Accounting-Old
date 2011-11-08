using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using Naviam.DAL;

namespace Naviam.Domain.Concrete
{
    public class BudgetRepository
    {
        private const string CacheKey = "userBudgets";

        public void ResetCache(int? companyId)
        {
            var cache = new CacheWrapper();
            cache.SetList<Budget>(CacheKey, null, companyId);
        }

        public virtual IEnumerable<Budget> GetBudgets(int? companyId) { return GetBudgets(companyId, false); }
        public virtual IEnumerable<Budget> GetBudgets(int? companyId, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<Budget>(CacheKey, companyId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                res = BudgetDataAdapter.GetBudgets(companyId);
                //save to cache
                cache.SetList(CacheKey, res, companyId);
            }
            return res;
        }
    }
}
