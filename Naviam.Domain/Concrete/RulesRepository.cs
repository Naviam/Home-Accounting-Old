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

        public void ResetCache(int? userId)
        {
            var cache = new CacheWrapper();
            cache.SetList<FieldRule>(CacheKey, null, userId);
        }

        public virtual List<FieldRule> GetUserRules(int? userId) { return GetUserRules(userId, false); }
        public virtual List<FieldRule> GetUserRules(int? userId, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<FieldRule>(CacheKey, userId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                res = RulesDataAdapter.GetUserRules(userId);
                //save to cache
                cache.SetList(CacheKey, res, userId);
            }
            return res;
        }

        public virtual FieldRule GetRule(int? id, int? userId) { return GetRule(id, userId, false); }
        public virtual FieldRule GetRule(int? id, int? userId, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetFromList(CacheKey, new FieldRule() { Id = id }, userId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                res = RulesDataAdapter.GetRule(id, userId);
                //save to cache
                if (res == null) // not found in cache->add
                    cache.AddToList<FieldRule>(CacheKey, res, userId);
                else
                    cache.UpdateList(CacheKey, res, userId);
            }
            return res;
        }

        private int InsertUpdate(FieldRule entity, int? userId, DbActionType action, bool intoCache)
        {
            var cache = new CacheWrapper();
            var res = RulesDataAdapter.InsertUpdate(entity, userId, action);
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

        public virtual int Insert(FieldRule entity, int? userId) { return Insert(entity, userId, true); }
        public virtual int Insert(FieldRule entity, int? userId, bool intoCache)
        {
            return InsertUpdate(entity, userId, DbActionType.Insert, intoCache);
        }

        public virtual int Update(FieldRule entity, int? userId)
        {
            return InsertUpdate(entity, userId, DbActionType.Update, true);
        }

        public virtual int Delete(int? id, int? userId)
        {
            var res = RulesDataAdapter.Delete(id, userId);
            if (res == 0)
            {
                //if ok - remove from cache
                new CacheWrapper().RemoveFromList2(CacheKey, new FieldRule() { Id = id }, userId);
            }
            return res;
        }


        public virtual string GetValueByRules(string targetField, string targetFieldValue, string field, int? userId)
        { return GetValueByRules(targetField, targetFieldValue, field, userId, false); }
        public virtual string GetValueByRules(string targetField, string targetFieldValue, string field, int? userId, bool forceSqlLoad)
        {
            string result = string.Empty;
            List<FieldRule> rules = GetUserRules(userId);
            foreach (FieldRule rule in rules)
            {
                if (rule.RuleType == RuleTypes.Equals)
                {
                    if (rule.FieldTarget.Equals(targetField, StringComparison.InvariantCultureIgnoreCase)
                        && rule.Field.Equals(field, StringComparison.InvariantCultureIgnoreCase)
                        && rule.FieldTargetValue.Equals(targetFieldValue, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return rule.FieldValue;
                    }
                }
                else if (rule.RuleType == RuleTypes.Regex)
                {
                    if (rule.FieldTarget.Equals(targetField, StringComparison.InvariantCultureIgnoreCase)
                        && rule.Field.Equals(field, StringComparison.InvariantCultureIgnoreCase)
                        && Regex.IsMatch(targetFieldValue, rule.FieldTargetValue, RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase))
                    {
                        return rule.FieldValue;
                    }
                }
            }//foreach

            return result;
        }

        public virtual string FindDescriptionMerchant(int? userId, string merchant)
        {
            //get description by user rules
            string val = GetValueByRules("merchant", merchant, "description", userId);
            if (!string.IsNullOrEmpty(val)) 
                return val;
            return merchant;
        }
    }
}
