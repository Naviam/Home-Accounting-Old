using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using Naviam.Code;
using System.Data.SqlClient;
using System.Data;

namespace Naviam.DAL.DAL
{
    public class CategoriesDataAdapter
    {
        private const string CacheKey = "transCategory";

        public static IEnumerable<Category> GetCategories(int? userId) { return GetCategories(userId, false); }
        public static IEnumerable<Category> GetCategories(int? userId, bool forceSqlLoad)
        {
            List<Category> res = CacheWrapper.GetList<Category>(CacheKey, userId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                using (SqlConnectionHolder holder = SqlConnectionHelper.GetConnection(SqlConnectionHelper.ConnectionType.Naviam))
                {
                    using (SqlCommand cmd = holder.Connection.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 120;
                        cmd.CommandText = "get_categories";
                        cmd.Parameters.AddWithValue("@id_company", userId);
                        try
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                res = new Categories(reader);
                            }
                        }
                        catch (SqlException e)
                        {
                            throw e;
                        }
                    }
                }
                //save to cache
                CacheWrapper.SetList<Category>(CacheKey, res, userId);
            }
            return res;
        }
    }
}
