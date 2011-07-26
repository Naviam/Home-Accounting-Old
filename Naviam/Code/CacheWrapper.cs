using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Configuration;
using ServiceStack.Redis;
using System.Web.WebPages;

namespace Naviam.Code
{
    public class CacheWrapper
    {

        //Tests:
        //Load of 300000    Save of 300000
        //simple-4900ms     simple-3200ms
        //list-4600ms       list-15300ms

        #region Single obj

        public static T Get<T>(string key) { return Get<T>(key, null); }
        public static T Get<T>(string key, int? id)
        {
            T res = default(T);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = new RedisClient(ConfigurationManager.AppSettings["RedisHost"], Convert.ToInt32(ConfigurationManager.AppSettings["RedisPort"])))
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                    if (id != null)
                        key = id + "_" + key;
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
        public static T GetAndSet<T>(string key, T val, int? id)
        {
            T res = default(T);
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = new RedisClient(ConfigurationManager.AppSettings["RedisHost"], Convert.ToInt32(ConfigurationManager.AppSettings["RedisPort"])))
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                    res = typedRedis[key];
                    HttpSessionState sess = null;
                    if (id != null)
                    {
                        key = id + "_" + key;
                        sess = HttpContext.Current.Session;
                    }
                    if (sess != null)
                    {
                        //session style
                        TimeSpan exp = new TimeSpan(0, sess.Timeout, 0);
                        using (redisClient.AcquireLock(key))
                            typedRedis.SetEntry(key, val, exp);
                    }
                    else
                        using (redisClient.AcquireLock(key))
                            typedRedis[key] = val;
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

        public static void Set<T>(string key, T val) { Set<T>(key, val, null); }
        public static void Set<T>(string key, T val, int? id)
        {
            if (ConfigurationManager.AppSettings["EnableRedis"].AsBool())
            {
                using (var redisClient = new RedisClient(ConfigurationManager.AppSettings["RedisHost"], Convert.ToInt32(ConfigurationManager.AppSettings["RedisPort"])))
                {
                    var typedRedis = redisClient.GetTypedClient<T>();
                    HttpSessionState sess = null;
                    if (id != null)
                    {
                        key = id + "_" + key;
                        sess = HttpContext.Current.Session;
                    }
                    if (sess != null)
                    {
                        //session style
                        TimeSpan exp = new TimeSpan(0, sess.Timeout, 0);
                        using (redisClient.AcquireLock(key))
                            typedRedis.SetEntry(key, val, exp);
                    }
                    else
                        using (redisClient.AcquireLock(key))
                            typedRedis[key] = val;
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

        #endregion

        #region Lists

        #endregion
    }
}