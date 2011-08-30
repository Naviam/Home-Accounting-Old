using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using Naviam.DAL;

namespace Naviam.Domain.Concrete
{
    public class FinanceInstitutionRepository
    {
        private const string CacheKey = "financeInstitution";
        private const string CacheKeyLink = "financeInstitutionLinkToAcc";

        public void ResetCache()
        {
            var cache = new CacheWrapper();
            cache.SetList<AccountType>(CacheKey, null);
        }

        public static List<FinanceInstitution> Get() { return Get(false); }
        public static List<FinanceInstitution> Get(bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<FinanceInstitution>(CacheKey);
            if (res == null || forceSqlLoad)
            {
                res = FinanceInstitutionDataAdapter.Get();
                //save to cache
                cache.SetList(CacheKey, res);
            }
            return res;
        }

        public static List<FinanceInstitutionLinkToAccount> GetLinksToAccount() { return GetLinksToAccount(false); }
        public static List<FinanceInstitutionLinkToAccount> GetLinksToAccount(bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<FinanceInstitutionLinkToAccount>(CacheKeyLink);
            if (res == null || forceSqlLoad)
            {
                res = FinanceInstitutionDataAdapter.GetLinksToAccount();
                //save to cache
                cache.SetList(CacheKeyLink, res);
            }
            return res;
        }
    }
}
