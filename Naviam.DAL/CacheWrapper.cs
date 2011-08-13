using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Configuration;
using ServiceStack.Redis;
using System.Web.WebPages;
using System.Web.Security;

namespace Naviam.WebUI
{
    public class CacheWrapper
    {

        //Tests:
        //Load of 300000    Save of 300000
        //simple-4900ms     simple-3200ms
        //list-4600ms       list-15300ms

        #region Single obj

        public static T Get<T>(string key) { return Get<T>(key, null); }
        public static T Get<T>(string key, params int?[] id)
        {
            T res = default(T);
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = new RedisClient(ConfigurationManager.AppSettings["RedisHost"], Convert.ToInt32(ConfigurationManager.AppSettings["RedisPort"])))
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                    res = typedRedis[key];
                }
            }
            else
            {
                object obj = null;
                if (id != null)
                {
                    obj = HttpContext.Current.Session[key];
                }
                else
                {
                    obj = HttpContext.Current.Cache[key];
                }
                if (obj != null)
                    res = (T)obj;
            }
            return res;
        }

        public static T GetAndSet<T>(string key, T val) { return GetAndSet<T>(key, val, null); }
        public static T GetAndSet<T>(string key, T val, params int?[] id)
        {
            T res = default(T);
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = new RedisClient(ConfigurationManager.AppSettings["RedisHost"], Convert.ToInt32(ConfigurationManager.AppSettings["RedisPort"])))
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                    res = typedRedis[key];
                    if (id != null)
                    {
                        //session style
                        TimeSpan exp = new TimeSpan(0, (int)FormsAuthentication.Timeout.TotalMinutes, 0);
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
                object obj = null;
                if (id != null)
                {
                    obj = HttpContext.Current.Session[key];
                    HttpContext.Current.Session[key] = val;
                }
                else
                {
                    obj = HttpContext.Current.Cache[key];
                    HttpContext.Current.Cache[key] = val;
                }
                if (obj != null)
                    res = (T)obj;
            }
            return res;
        }

        public static void ProlongKey(string key)
        {
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = new RedisClient(ConfigurationManager.AppSettings["RedisHost"], Convert.ToInt32(ConfigurationManager.AppSettings["RedisPort"])))
                {
                    TimeSpan exp = new TimeSpan(0, (int)FormsAuthentication.Timeout.TotalMinutes, 0);
                    redisClient.ExpireEntryIn(key, exp);
                }
            }
        }

        public static void Set<T>(string key, T val) { Set<T>(key, val, false, null); }
        public static void Set<T>(string key, T val, bool forceExpire, params int?[] id)
        {
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = new RedisClient(ConfigurationManager.AppSettings["RedisHost"], Convert.ToInt32(ConfigurationManager.AppSettings["RedisPort"])))
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                    if (id != null || forceExpire)
                    {
                        //session style
                        TimeSpan exp = new TimeSpan(0, (int)FormsAuthentication.Timeout.TotalMinutes, 0);
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
                {
                    if (id != null)
                        HttpContext.Current.Session[key] = val;
                    else
                        HttpContext.Current.Cache[key] = val;
                }
                else
                {
                    if (id != null)
                        HttpContext.Current.Session.Remove(key);
                    else
                        HttpContext.Current.Cache.Remove(key);
                }
            }
        }

        #endregion

        #region Lists

        public static List<T> GetList<T>(string key) { return GetList<T>(key, null); }
        public static List<T> GetList<T>(string key, params int?[] id)
        {
            List<T> res = null;
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = new RedisClient(ConfigurationManager.AppSettings["RedisHost"], Convert.ToInt32(ConfigurationManager.AppSettings["RedisPort"])))
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
                object obj = null;
                if (id != null)
                {
                    obj = HttpContext.Current.Session[key];
                }
                else
                {
                    obj = HttpContext.Current.Cache[key];
                }
                if (obj != null)
                    res = (List<T>)obj;
            }
            return res;
        }

        public static void SetList<T>(string key, List<T> val) { SetList<T>(key, val, null); }
        public static void SetList<T>(string key, List<T> val, params int?[] id)
        {
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = new RedisClient(ConfigurationManager.AppSettings["RedisHost"], Convert.ToInt32(ConfigurationManager.AppSettings["RedisPort"])))
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                    
                    using (redisClient.AcquireLock(key + "lock"))
                    {
                        var list = typedRedis.Lists[key];
                        list.Clear();
                        val.ForEach(x => list.Add(x));
                    }
                    if (id != null)
                    {
                        //session style
                        TimeSpan exp = new TimeSpan(0, (int)FormsAuthentication.Timeout.TotalMinutes, 0);
                        typedRedis.ExpireEntryIn(key, exp);
                    }
                }
            }
            else
            {
                if (id != null)
                    HttpContext.Current.Session[key] = val;
                else
                    HttpContext.Current.Cache[key] = val;
            }
        }

        public static void AddToList<T>(string key, T val) { AddToList<T>(key, val, null); }
        public static void AddToList<T>(string key, T val, params int?[] id)
        {
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = new RedisClient(ConfigurationManager.AppSettings["RedisHost"], Convert.ToInt32(ConfigurationManager.AppSettings["RedisPort"])))
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
                        TimeSpan exp = new TimeSpan(0, (int)FormsAuthentication.Timeout.TotalMinutes, 0);
                        typedRedis.ExpireEntryIn(key, exp);
                    }
                }
            }
            else
            {
                object obj = null;
                if (id != null)
                    obj = HttpContext.Current.Session[key];
                else
                    obj = HttpContext.Current.Cache[key];
                if (obj != null)
                {
                    ((List<T>)obj).Add(val);
                }
            }
        }

        public static void UpdateList<T>(string key, T val) { UpdateList<T>(key, val, null); }
        public static void UpdateList<T>(string key, T val, params int?[] id)
        {
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = new RedisClient(ConfigurationManager.AppSettings["RedisHost"], Convert.ToInt32(ConfigurationManager.AppSettings["RedisPort"])))
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                    
                    using (redisClient.AcquireLock(key + "lock"))
                    {
                        var list = typedRedis.Lists[key];
                        int index = list.IndexOf(val);
                        if (index != -1)
                            list[index] = val;
                    }
                    if (id != null)
                    {
                        //session style
                        TimeSpan exp = new TimeSpan(0, (int)FormsAuthentication.Timeout.TotalMinutes, 0);
                        typedRedis.ExpireEntryIn(key, exp);
                    }
                }
            }
            else
            {
                object obj = null;
                if (id != null)
                    obj = HttpContext.Current.Session[key];
                else
                    obj = HttpContext.Current.Cache[key];
                if (obj != null)
                {
                    List<T> lst = (List<T>)obj;
                    for (int i = 0; i < lst.Count; i++)
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

        public static void RemoveFromList<T>(string key, T val) { RemoveFromList<T>(key, val, null); }
        public static void RemoveFromList<T>(string key, T val, params int?[] id)
        {
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = new RedisClient(ConfigurationManager.AppSettings["RedisHost"], Convert.ToInt32(ConfigurationManager.AppSettings["RedisPort"])))
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
                object obj = null;
                if (id != null)
                    obj = HttpContext.Current.Session[key];
                else
                    obj = HttpContext.Current.Cache[key];
                if (obj != null)
                {
                    List<T> lst = (List<T>)obj;
                    lst.Remove(val);
                }
            }
        }

        public static T GetFromList<T>(string key, T val) { return GetFromList<T>(key, val, null); }
        public static T GetFromList<T>(string key, T val, params int?[] id)
        {
            T res = default(T);
            key = GetKey(key, id);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = new RedisClient(ConfigurationManager.AppSettings["RedisHost"], Convert.ToInt32(ConfigurationManager.AppSettings["RedisPort"])))
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                   
                    var list = typedRedis.Lists[key];
                    int index = list.IndexOf(val);
                    if (index != -1)
                        res = list[index];
                }
            }
            else
            {
                object obj = null;
                if (id != null)
                    obj = HttpContext.Current.Session[key];
                else
                    obj = HttpContext.Current.Cache[key];
                if (obj != null)
                {
                    List<T> lst = (List<T>)obj;
                    for (int i = 0; i < lst.Count; i++)
                    {
                        var existingItem = lst[i];
                        if (existingItem.Equals(val))
                        {
                            res = lst[i];
                            break;
                        }
                    }
                }
            }
            return res;
        }


        private static string GetKey(string key, params int?[] id)
        {
            if (id != null)
            {
                for (int i = 0; i < id.Length; i++)
                {
                    if (id[i].HasValue)
                        key = key + "_" + id[i].Value;
                }
            }
            return key;
        }
        #endregion
    }
}