using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    public class CurrenciesDataAdapter
    {
        private const string CacheKey = "currensies";

        public static List<Currency> GetCurrencies() { return GetCurrencies(false); }
        public static List<Currency> GetCurrencies(bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<Currency>(CacheKey);
            if (res == null || forceSqlLoad)
            {
                res = new List<Currency>();
                using (var holder = SqlConnectionHelper.GetConnection())
                {
                    using (var cmd = holder.Connection.CreateSPCommand("currencies_get"))
                    {
                        try
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    res.Add(new Currency(reader));
                            }
                        }
                        catch (SqlException e)
                        {
                            cmd.AddDetailsToException(e);
                            throw;
                        }
                    }
                }
                //save to cache
                cache.SetList(CacheKey, res);
            }
            return res;
        }

    }
}
