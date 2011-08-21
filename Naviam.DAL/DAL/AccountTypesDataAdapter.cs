using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    public class AccountTypesDataAdapter
    {

        private const string CacheKey = "accountType";

        public static List<AccountType> GetAccountTypes() { return GetAccountTypes(false); }
        public static List<AccountType> GetAccountTypes(bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<AccountType>(CacheKey);
            if (res == null || forceSqlLoad)
            {
                res = new List<AccountType>();
                using (var holder = SqlConnectionHelper.GetConnection())
                {
                    using (var cmd = holder.Connection.CreateSPCommand("account_types_get"))
                    {
                        try
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    res.Add(new AccountType(reader));
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
