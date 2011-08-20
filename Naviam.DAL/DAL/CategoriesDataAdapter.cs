using System.Collections.Generic;
using Naviam.Data;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    public class CategoriesDataAdapter
    {
        public static List<Category> GetCategories(int? userId)
        {
            var res = new List<Category>();
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
            return res;
        }
    }

}
