using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using ServiceStack.Redis;
using System.Web.WebPages;
using System.Web.Security;

namespace Naviam
{
    public interface ICacheWrapper
    {
        T Get<T>(string key);
        T Get<T>(string key, params int?[] id);
        T GetAndSet<T>(string key, T val);
        T GetAndSet<T>(string key, T val, params int?[] id);
        void ProlongKey(string key);
        void Set<T>(string key, T val);
        void Set<T>(string key, T val, bool forceExpire, params int?[] id);
        List<T> GetList<T>(string key);
        List<T> GetList<T>(string key, params int?[] id);
        void SetList<T>(string key, List<T> val);
        void SetList<T>(string key, List<T> val, params int?[] id);
        void AddToList<T>(string key, T val);
        void AddToList<T>(string key, T val, params int?[] id);
        void UpdateList<T>(string key, T val);
        void UpdateList<T>(string key, T val, params int?[] id);
        void RemoveFromList<T>(string key, T val);
        void RemoveFromList<T>(string key, T val, params int?[] id);
        T GetFromList<T>(string key, T val);
        T GetFromList<T>(string key, T val, params int?[] id);
    }


    public class CacheWrapper : ICacheWrapper
    {
        //Tests:
        //Load of 300000    Save of 300000
        //simple-4900ms     simple-3200ms
        //list-4600ms       list-15300ms

        public static PooledRedisClientManager ClientManager 
        {
            get 
            {
                var host = ConfigurationManager.AppSettings["RedisHost"];
                var port = ConfigurationManager.AppSettings["RedisPort"];
                return new PooledRedisClientManager(host + ":" + port);
            }
        }

        #region Single obj

        public T Get<T>(string key) { return Get<T>(key, null); }
        public T Get<T>(string key, params int?[] id)
        {
            var res = default(T);
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = ClientManager.GetClient())
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                    res = typedRedis[key];
                }
            }
            else
            {
                var obj = HttpContext.Current.Cache[key];
                //var obj = id != null ? HttpContext.Current.Session[key] : HttpContext.Current.Cache[key];
                if (obj != null)
                    res = (T)obj;
            }
            return res;
        }

        public T GetAndSet<T>(string key, T val) { return GetAndSet(key, val, null); }
        public T GetAndSet<T>(string key, T val, params int?[] id)
        {
            var res = default(T);
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = ClientManager.GetClient())
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                    res = typedRedis[key];
                    if (id != null)
                    {
                        //session style
                        var exp = new TimeSpan(0, (int)FormsAuthentication.Timeout.TotalMinutes, 0);
                        using (redisClient.AcquireLock(key + "lock"))
                        {
                            if (val == null)
                                typedRedis.RemoveEntry(key);
                            else
                                typedRedis.SetEntry(key, val, exp);
                        }
                    }
                    else
                        using (redisClient.AcquireLock(key + "lock"))
                        {
                            if (val == null)
                                typedRedis.RemoveEntry(key);
                            else
                                typedRedis[key] = val;
                        }
                }
            }
            else
            {
                object obj;
                obj = HttpContext.Current.Cache[key];
                HttpContext.Current.Cache[key] = val;
                if (obj != null)
                    res = (T)obj;
            }
            return res;
        }

        public void ProlongKey(string key)
        {
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = ClientManager.GetClient())
                {
                    var exp = new TimeSpan(0, (int)FormsAuthentication.Timeout.TotalMinutes, 0);
                    redisClient.ExpireEntryIn(key, exp);
                }
            }
        }

        public void Set<T>(string key, T val) { Set(key, val, false, null); }
        public void Set<T>(string key, T val, bool forceExpire, params int?[] id)
        {
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = ClientManager.GetClient())
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                    if (id != null || forceExpire)
                    {
                        //session style
                        var exp = new TimeSpan(0, (int)FormsAuthentication.Timeout.TotalMinutes, 0);
                        using (redisClient.AcquireLock(key + "lock"))
                        {
                            if (val == null)
                                typedRedis.RemoveEntry(key);
                            else
                                typedRedis.SetEntry(key, val, exp);
                        }
                    }
                    else
                        using (redisClient.AcquireLock(key + "lock"))
                        {
                            if (val == null)
                                typedRedis.RemoveEntry(key);
                            else
                                typedRedis[key] = val;
                        }
                }
            }
            else
            {
                if (val != null)
                    HttpContext.Current.Cache[key] = val;
                else
                    HttpContext.Current.Cache.Remove(key);
            }
        }

        #endregion

        #region Lists

        public List<T> GetList<T>(string key) { return GetList<T>(key, null); }
        public List<T> GetList<T>(string key, params int?[] id)
        {
            List<T> res = null;
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = ClientManager.GetClient())
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                    var list = typedRedis.Lists[key];
                    res = list.GetAll();
                    if (res.Count == 0)
                        res = null;
                }
            }
            else
            {
                var obj = HttpContext.Current.Cache[key];
                if (obj != null)
                    res = (List<T>)obj;
            }
            return res;
        }

        public void SetList<T>(string key, List<T> val) { SetList(key, val, null); }
        public void SetList<T>(string key, List<T> val, params int?[] id)
        {
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = ClientManager.GetClient())
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                    
                    using (redisClient.AcquireLock(key + "lock"))
                    {
                        var list = typedRedis.Lists[key];
                        list.Clear();
                        if (val != null)
                            val.ForEach(list.Add);
                    }
                    if (id != null)
                    {
                        //session style
                        var exp = new TimeSpan(0, (int)FormsAuthentication.Timeout.TotalMinutes, 0);
                        typedRedis.ExpireEntryIn(key, exp);
                    }
                }
            }
            else
            {
                HttpContext.Current.Cache[key] = val;
            }
        }

        public void AddToList<T>(string key, T val) { AddToList(key, val, null); }
        public void AddToList<T>(string key, T val, params int?[] id)
        {
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = ClientManager.GetClient())
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                    
                    using (redisClient.AcquireLock(key + "lock"))
                    {
                        var list = typedRedis.Lists[key];
                        list.Add(val);
                    }
                    if (id != null)
                    {
                        //session style
                        var exp = new TimeSpan(0, (int)FormsAuthentication.Timeout.TotalMinutes, 0);
                        typedRedis.ExpireEntryIn(key, exp);
                    }
                }
            }
            else
            {
                var obj = HttpContext.Current.Cache[key];
                if (obj == null)
                {
                    obj = new List<T>();
                    HttpContext.Current.Cache[key] = obj;
                }
                ((List<T>)obj).Add(val);
            }
        }

        public void UpdateList<T>(string key, T val) { UpdateList(key, val, null); }
        public void UpdateList<T>(string key, T val, params int?[] id)
        {
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = ClientManager.GetClient())
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                    
                    using (redisClient.AcquireLock(key + "lock"))
                    {
                        var list = typedRedis.Lists[key];
                        var index = list.IndexOf(val);
                        if (index != -1)
                            list[index] = val;
                    }
                    if (id != null)
                    {
                        //session style
                        var exp = new TimeSpan(0, (int)FormsAuthentication.Timeout.TotalMinutes, 0);
                        typedRedis.ExpireEntryIn(key, exp);
                    }
                }
            }
            else
            {
                var obj = HttpContext.Current.Cache[key];
                if (obj != null)
                {
                    var lst = (List<T>)obj;
                    for (var i = 0; i < lst.Count; i++)
                    {
                        var existingItem = lst[i];
                        if (existingItem.Equals(val))
                        {
                            lst[i] = val;
                            break;
                        }
                    }
                }
            }
        }

        public void RemoveFromList<T>(string key, T val) { RemoveFromList(key, val, null); }
        public void RemoveFromList<T>(string key, T val, params int?[] id)
        {
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = ClientManager.GetClient())
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                    
                    using (redisClient.AcquireLock(key + "lock"))
                    {
                        var list = typedRedis.Lists[key];
                        list.Remove(val);
                    }
                }
            }
            else
            {
                var obj = HttpContext.Current.Cache[key];
                if (obj != null)
                {
                    var lst = (List<T>)obj;
                    lst.Remove(val);
                }
            }
        }

        public void RemoveFromList2<T>(string key, T val) { RemoveFromList2(key, val, null); }
        public void RemoveFromList2<T>(string key, T val, params int?[] id)
        {
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = ClientManager.GetClient())
                {
                    var typedRedis = redisClient.GetTypedClient<T>();

                    using (redisClient.AcquireLock(key + "lock"))
                    {
                        var list = typedRedis.Lists[key];
                        var index = list.IndexOf(val);
                        if (index != -1)
                            list.RemoveAt(index);
                    }
                }
            }
            else
            {
                var obj = HttpContext.Current.Cache[key];
                if (obj != null)
                {
                    var lst = (List<T>)obj;
                    for (var i = 0; i < lst.Count; i++)
                    {
                        var existingItem = lst[i];
                        if (existingItem.Equals(val))
                        {
                            lst.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        public T GetFromList<T>(string key, T val) { return GetFromList(key, val, null); }
        public T GetFromList<T>(string key, T val, params int?[] id)
        {
            var res = default(T);
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = ClientManager.GetClient())
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                   
                    var list = typedRedis.Lists[key];
                    var index = list.IndexOf(val);
                    if (index != -1)
                        res = list[index];
                }
            }
            else
            {
                var obj = HttpContext.Current.Cache[key];
                if (obj != null)
                {
                    var lst = (List<T>)obj;
                    foreach (var t in from t in lst let existingItem = t where existingItem.Equals(val) select t)
                    {
                        res = t;
                        break;
                    }
                }
            }
            return res;
        }


        private static string GetKey(string key, params int?[] id)
        {
            if (id != null)
            {
                key = id.Where(t => t.HasValue).Aggregate(key, (current, t) => t != null ? current + "_" + t.Value : null);
            }
            return key;
        }
        #endregion
    }
}