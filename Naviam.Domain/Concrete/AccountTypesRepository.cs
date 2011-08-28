using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using Naviam.DAL;

namespace Naviam.Domain.Concrete
{
    public class AccountTypesRepository
    {
        private const string CacheKey = "accountType";

        public void ResetCache()
        {
            var cache = new CacheWrapper();
            cache.SetList<AccountType>(CacheKey, null);
        }

        public static List<AccountType> GetAccountTypes() { return GetAccountTypes(false); }
        public static List<AccountType> GetAccountTypes(bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<AccountType>(CacheKey);
            if (res == null || forceSqlLoad)
            {
                res = AccountTypesDataAdapter.GetAccountTypes();
                //save to cache
                cache.SetList(CacheKey, res);
            }
            return res;
        }
    }
}
