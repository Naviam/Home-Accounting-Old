using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using Naviam.WebUI;
using System.Data.SqlClient;
using System.Data;

namespace Naviam.DAL
{
    public class CategoriesDataAdapter
    {
        private const string CacheKey = "transCategory";

        public static List<Category> GetCategories(int? userId) { return GetCategories(userId, false); }
        public static List<Category> GetCategories(int? userId, bool forceSqlLoad)
        {
            List<Category> res = CacheWrapper.GetList<Category>(CacheKey, userId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                res = new List<Category>();
                using (SqlConnectionHolder holder = SqlConnectionHelper.GetConnection(SqlConnectionHelper.ConnectionType.Naviam))
                {
                    using (SqlCommand cmd = holder.Connection.CreateSPCommand("get_categories"))
                    {
                        cmd.Parameters.AddWithValue("@id_user", userId);
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
