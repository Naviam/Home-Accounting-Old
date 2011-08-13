using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.WebUI;
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
            List<Company> res = CacheWrapper.GetList<Company>(CacheKey, userId);
            if (res == null || forceSqlLoad)
            {
                res = new List<Company>();
                using (SqlConnectionHolder holder = SqlConnectionHelper.GetConnection(SqlConnectionHelper.ConnectionType.Naviam))
                {
                    using (SqlCommand cmd = holder.Connection.CreateSPCommand("companies_get"))
                    {
                        cmd.Parameters.AddWithValue("@id_user", userId);
                        try
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    res.Add(new Company(reader));
                            }
                        }
                        catch (SqlException e)
                        {
                            throw e;
                        }
                    }
                }
                //save to cache
                CacheWrapper.SetList<Company>(CacheKey, res, userId);
            }
            return res;
        }

    }
}
