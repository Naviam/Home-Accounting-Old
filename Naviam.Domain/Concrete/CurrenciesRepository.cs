using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using Naviam.DAL;

namespace Naviam.Domain.Concrete
{
    public class CurrenciesRepository
    {
        private const string CacheKey = "currensies";

        public static List<Currency> GetCurrencies() { return GetCurrencies(false); }
        public static List<Currency> GetCurrencies(bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<Currency>(CacheKey);
            if (res == null || forceSqlLoad)
            {
                res = CurrenciesDataAdapter.GetCurrencies();
                //save to cache
                cache.SetList(CacheKey, res);
            }
            return res;
        }
    }
}
