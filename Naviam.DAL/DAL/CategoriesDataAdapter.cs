using System.Collections.Generic;
using Naviam.Data;
using System.Data.SqlClient;
using System.Data;

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

        public static int InsertUpdate(Category entity, int? userId, DbActionType action)
        {
            var res = -1;
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                var commName = action == DbActionType.Insert ? "category_create" : "web.category_update";
                var cmd = holder.Connection.CreateSPCommand(commName);
                try
                {
                    cmd.AddEntityParameters(entity, action);
                    cmd.ExecuteNonQuery();
                    if (action == DbActionType.Insert)
                        entity.Id = cmd.GetRowIdParameter();
                    res = cmd.GetReturnParameter();
                }
                catch (SqlException e)
                {
                    cmd.AddDetailsToException(e);
                    throw;
                }
            }
            return res;
        }

        public static int Delete(int? id, int? userId)
        {
            var res = -1;
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("category_delete"))
                {
                    try
                    {
                        cmd.AddCommonParameters(id);
                        cmd.ExecuteNonQuery();
                        res = cmd.GetReturnParameter();
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

        public static int? FindCategoryForMerchant(int? id_account, string merchant)
        {
            int? res = -1;
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                var commName = "merchant_find_category";
                var cmd = holder.Connection.CreateSPCommand(commName);
                try
                {
                    cmd.Parameters.Add("@id_category", SqlDbType.Int).Direction = ParameterDirection.InputOutput;
                    cmd.Parameters.AddWithValue("@id_account", id_account);
                    cmd.Parameters.AddWithValue("@merchant", merchant);
                    cmd.ExecuteNonQuery();
                    res = cmd.GetRowIdParameter("@id_category");
                }
                catch (SqlException e)
                {
                    cmd.AddDetailsToException(e);
                    throw;
                }
            }
            return res;
        }

        public static List<CategoryMerchant> GetCategoriesMerchant()
        {
            var res = new List<CategoryMerchant>();
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("categories_merchants_get"))
                {
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                res.Add(new CategoryMerchant(reader));
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

        public static CategoryMerchant GetUsersCategoryMerchant(int? id_account, string merchant)
        {
            CategoryMerchant res = null;
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                var commName = "categories_merchants_user_get";
                var cmd = holder.Connection.CreateSPCommand(commName);
                try
                {
                    cmd.Parameters.Add("@id_category", SqlDbType.Int).Direction = ParameterDirection.InputOutput;
                    cmd.Parameters.AddWithValue("@id_account", id_account);
                    cmd.Parameters.AddWithValue("@merchant", merchant);
                    cmd.ExecuteNonQuery();
                    int? categoryId = cmd.GetRowIdParameter("@id_category");
                    res = categoryId.HasValue ? new CategoryMerchant(merchant, categoryId.Value) : null;
                }
                catch (SqlException e)
                {
                    cmd.AddDetailsToException(e);
                    throw;
                }
            }
            return res;
        }

        public static List<CategoryRule> GetCategoriesRules()
        {
            var res = new List<CategoryRule>();
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("categories_rules_get"))
                {
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                res.Add(new CategoryRule(reader));
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
