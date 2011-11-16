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

        public static FieldRule GetRule(int? id, int? companyId) { return GetRule(id, companyId, false); }
        public static FieldRule GetRule(int? id, int? companyId, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetFromList(CacheKey, new FieldRule() { Id = id }, companyId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                res = RulesDataAdapter.GetRule(id, companyId);
                //save to cache
                if (res == null) // not found in cache->add
                    cache.AddToList<FieldRule>(CacheKey, res, companyId);
                else
                    cache.UpdateList(CacheKey, res, companyId);
            }
            return res;
        }

        private static int InsertUpdate(FieldRule entity, int? companyId, DbActionType action, bool intoCache)
        {
            var cache = new CacheWrapper();
            var res = RulesDataAdapter.InsertUpdate(entity, companyId, action);
            if (res == 0)
            {
                //if ok - update cache
                if (intoCache)
                {
                    if (action == DbActionType.Insert)
                        cache.AddToList(CacheKey, entity, companyId);
                    if (action == DbActionType.Update)
                        cache.UpdateList(CacheKey, entity, companyId);
                }
            }
            return res;
        }

        public static int Insert(FieldRule entity, int? companyId) { return Insert(entity, companyId, true); }
        public static int Insert(FieldRule entity, int? companyId, bool intoCache)
        {
            return InsertUpdate(entity, companyId, DbActionType.Insert, intoCache);
        }

        public static int Update(FieldRule entity, int? companyId)
        {
            return InsertUpdate(entity, companyId, DbActionType.Update, true);
        }

        public static int Delete(FieldRule entity, int? companyId)
        {
            var res = RulesDataAdapter.Delete(entity, companyId);
            if (res == 0)
            {
                //if ok - remove from cache
                new CacheWrapper().RemoveFromList(CacheKey, entity, companyId);
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
                if (rule.RuleType == RuleTypes.Equals)
                {
                    if (rule.FildTarget.Equals(targetField, StringComparison.InvariantCultureIgnoreCase)
                        && rule.Fild.Equals(field, StringComparison.InvariantCultureIgnoreCase)
                        && rule.FildTargetValue.Equals(targetFieldValue, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return rule.FildValue;
                    }
                }
                else if (rule.RuleType == RuleTypes.Regex)
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

        public virtual string FindDescriptionMerchant(int? idCompany, string merchant)
        {
            //get description by user rules
            string val = GetValueByRules("merchant", merchant, "description", idCompany);
            if (!string.IsNullOrEmpty(val)) 
                return val;
            return merchant;
        }
    }
}
