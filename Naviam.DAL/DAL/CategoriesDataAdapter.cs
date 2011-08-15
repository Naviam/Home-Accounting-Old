using System.Collections.Generic;
using Naviam.Data;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    public class CategoriesDataAdapter
    {
        private const string CacheKey = "transCategory";

        public static List<Category> GetCategories(int? userId) { return GetCategories(userId, false); }
        public static List<Category> GetCategories(int? userId, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<Category>(CacheKey, userId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                res = new List<Category>();
                using (var holder = SqlConnectionHelper.GetConnection())
                {
                    using (var cmd = holder.Connection.CreateSPCommand("categories_get"))
                    {
                        cmd.Parameters.AddWithValue("@id_user", userId);
                        try
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                res = new Categories(reader);
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
