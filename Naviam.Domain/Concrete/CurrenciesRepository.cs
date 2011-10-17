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

        public void ResetCache()
        {
            var cache = new CacheWrapper();
            cache.SetList<Currency>(CacheKey, null);
        }

        public List<Currency> GetCurrencies() { return GetCurrencies(false); }
        public List<Currency> GetCurrencies(bool forceSqlLoad)
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

        public virtual Currency GetCurrencyByShortName(string shortName) { return GetCurrencyByShortName(shortName,false); }
        public virtual Currency GetCurrencyByShortName(string shortName, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var list = cache.GetList<Currency>(CacheKey);
            if (list == null || forceSqlLoad)
            {
                list = CurrenciesDataAdapter.GetCurrencies();
                //save to cache
                cache.SetList(CacheKey, list);
            }
            return list.FirstOrDefault(x=>x.NameShort == shortName);
        }
    }
}
