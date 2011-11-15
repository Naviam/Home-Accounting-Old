using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using Naviam.DAL;
using System.Text.RegularExpressions;

namespace Naviam.Domain.Concrete
{
    public class RulesRepository
    {
        private const string CacheKey = "accountRules";

        public void ResetCache(int? companyId)
        {
            var cache = new CacheWrapper();
            cache.SetList<FieldRule>(CacheKey, null, companyId);
        }

        public virtual List<FieldRule> GetUserRules(int? companyId) { return GetUserRules(companyId, false); }
        public virtual List<FieldRule> GetUserRules(int? companyId, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<FieldRule>(CacheKey, companyId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                res = RulesDataAdapter.GetUserRules(companyId);
                //save to cache
                cache.SetList(CacheKey, res, companyId);
            }
            return res;
        }
        public virtual string GetValueByRules(string targetField, string targetFieldValue, string field, int? companyId)
        { return GetValueByRules(targetField, targetFieldValue, field, companyId, false); }
        public virtual string GetValueByRules(string targetField, string targetFieldValue, string field, int? companyId, bool forceSqlLoad)
        {
            string result = string.Empty;
            List<FieldRule> rules = GetUserRules(companyId);
            foreach (FieldRule rule in rules)
            {
                if (rule.RuleType.Value == RuleTypes.Equals)
                {
                    if (rule.FildTarget.Equals(targetField, StringComparison.InvariantCultureIgnoreCase)
                        && rule.Fild.Equals(field, StringComparison.InvariantCultureIgnoreCase)
                        && rule.FildTargetValue.Equals(targetFieldValue, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return rule.FildValue;
                    }
                }
                else if (rule.RuleType.Value == RuleTypes.Regex)
                {
                    if (rule.FildTarget.Equals(targetField, StringComparison.InvariantCultureIgnoreCase)
                        && rule.Fild.Equals(field, StringComparison.InvariantCultureIgnoreCase)
                        && Regex.IsMatch(targetFieldValue, rule.FildTargetValue, RegexOptions.Multiline | RegexOptions.CultureInvariant))
                    {
                        return rule.FildValue;
                    }
                }
            }
            return result;
        }
    }
}
