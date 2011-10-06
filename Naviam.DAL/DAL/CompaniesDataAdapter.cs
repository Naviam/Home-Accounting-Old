using System.Collections.Generic;
using System.Data.SqlClient;
using Naviam.Data;

namespace Naviam.DAL
{
    public class CompaniesDataAdapter
    {
        private const string CacheKey = "userCompanies";

        public static List<Company> GetCompanies(int? userId) { return GetCompanies(userId, false); }
        public static List<Company> GetCompanies(int? userId, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();

            var res = cache.GetList<Company>(CacheKey, userId);
            if (res == null || forceSqlLoad)
            {
                res = new List<Company>();
                using (var holder = SqlConnectionHelper.GetConnection())
                {
                    using (var cmd = holder.Connection.CreateSPCommand("companies_get"))
                    {
                        cmd.Parameters.AddWithValue("@id_user", userId);
                        try
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    res.Add(new Company(reader));
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
                cache.SetList(CacheKey, res, userId);
            }
            return res;
        }

    }
}
