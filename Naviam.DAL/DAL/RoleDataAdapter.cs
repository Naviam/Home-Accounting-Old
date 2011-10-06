using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    public class RoleDataAdapter
    {
        public static int CommandTimeout { get; set; }

        public static int CreateRole(string roleName)
        {
            using (var connection = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = connection.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandText = "role_create";

                    cmd.Parameters.AddWithValue("@role_name", roleName);
                    var returnParam = new SqlParameter("ReturnValue", SqlDbType.Int, 4)
                                          {
                                              Direction = ParameterDirection.ReturnValue
                                          };
                    cmd.Parameters.Add(returnParam);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        return (int)returnParam.Value;
                    }
                    catch (SqlException e)
                    {
                        cmd.AddDetailsToException(e);
                        throw;
                    }
                }
            }
        }

        public static int DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            using (var connection = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = connection.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandText = "role_del";

                    cmd.Parameters.AddWithValue("@role_name", roleName);
                    cmd.Parameters.AddWithValue("@delete_only_if_role_is_empty", throwOnPopulatedRole);
                    var returnParam = new SqlParameter("ReturnValue", SqlDbType.Int, 4)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParam);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        return (int)returnParam.Value;
                    }
                    catch (SqlException e)
                    {
                        cmd.AddDetailsToException(e);
                        throw;
                    }
                }
            }
        }

        public static string[] GetAllRoles()
        {
            using (var connection = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = connection.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandText = "roles_get";
                    
                    var roles = new List<string>();

                    try
                    {
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            roles.Add(reader.GetString(0));
                        }
                        return roles.ToArray();
                    }
                    catch (SqlException e)
                    {
                        cmd.AddDetailsToException(e);
                        throw;
                    }
                }
            }
        }

        public static int RoleExists(string roleName)
        {
            using (var connection = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = connection.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandText = "role_exists";

                    cmd.Parameters.AddWithValue("@role_name", roleName);
                    var returnParam = new SqlParameter("ReturnValue", SqlDbType.Int, 4)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParam);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        return (int) returnParam.Value;
                    }
                    catch (SqlException e)
                    {
                        cmd.AddDetailsToException(e);
                        throw;
                    }
                }
            }
        }

        public static int IsUserInRole(string userName, string roleName)
        {
            using (var connection = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = connection.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandText = "is_user_in_role";

                    cmd.Parameters.AddWithValue("@email", userName);
                    cmd.Parameters.AddWithValue("@role_name", roleName);
                    var returnParam = new SqlParameter("ReturnValue", SqlDbType.Int, 4)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParam);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        return (int)returnParam.Value;
                    }
                    catch (SqlException e)
                    {
                        cmd.AddDetailsToException(e);
                        throw;
                    }
                }
            }
        }

        public static string[] GetUsersInRole(string roleName, out int resultValue)
        {
            using (var connection = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = connection.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandText = "users_in_roles_get_users_in_role";

                    cmd.Parameters.AddWithValue("@role_name", roleName);
                    var returnParam = new SqlParameter("ReturnValue", SqlDbType.Int, 4)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParam);

                    var users = new List<string>();
                    try
                    {
                        var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                        while (reader.Read())
                        {
                            users.Add(reader.GetString(0));
                        }
                        resultValue = (int)returnParam.Value;
                        return users.ToArray();
                    }
                    catch (SqlException e)
                    {
                        cmd.AddDetailsToException(e);
                        throw;
                    }
                }
            }
        }

        public static List<string> GetRolesForUser(string userName)
        {
            var roles = new List<string>();
            using (var connection = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = connection.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandText = "users_in_roles_get_roles_for_user";

                    cmd.Parameters.AddWithValue("@email", userName);

                    try
                    {
                        var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                        while (reader.Read())
                        {
                            roles.Add(reader.GetString(0));
                        }
                        return roles;
                    }
                    catch (SqlException e)
                    {
                        cmd.AddDetailsToException(e);
                        throw;
                    }
                }
            }
        }

        public static string[] FindUsersInRole(string roleName, string usernameToMatch, out int resultValue)
        {
            using (var connection = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = connection.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandText = "users_in_roles_find_users_in_role";

                    cmd.Parameters.AddWithValue("@role_name", roleName);
                    cmd.Parameters.AddWithValue("@user_name_to_match", usernameToMatch);
                    var returnParam = new SqlParameter("ReturnValue", SqlDbType.Int, 4)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParam);

                    var roles = new List<string>();
                    try
                    {
                        var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                        while (reader.Read())
                        {
                            roles.Add(reader.GetString(0));
                        }
                        resultValue = (int)returnParam.Value;
                        return roles.ToArray();
                    }
                    catch (SqlException e)
                    {
                        cmd.AddDetailsToException(e);
                        throw;
                    }
                }
            }
        }

        public static string[] AddUsersToRoles(string userNames, string roleNames, out int resultValue)
        {
            using (var connection = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = connection.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandText = "users_in_roles_add_users_to_roles";

                    cmd.Parameters.AddWithValue("@emails", userNames);
                    cmd.Parameters.AddWithValue("@roles", roleNames);
                    cmd.Parameters.AddWithValue("@current_time_utc", DateTime.UtcNow);
                    var returnParam = new SqlParameter("ReturnValue", SqlDbType.Int, 4)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParam);

                    var roles = new List<string>();
                    try
                    {
                        var reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                        while (reader.Read())
                        {
                            roles.Add(reader.GetString(0));
                        }
                        resultValue = (int)returnParam.Value;
                        return roles.ToArray();
                    }
                    catch (SqlException e)
                    {
                        cmd.AddDetailsToException(e);
                        throw;
                    }
                }
            }
        }

        public static string[] RemoveUsersFromRoles(string userNames, string roleNames, out int resultValue)
        {
            using (var connection = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = connection.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandText = "users_in_roles_add_users_to_roles";

                    cmd.Parameters.AddWithValue("@emails", userNames);
                    cmd.Parameters.AddWithValue("@roles", roleNames);
                    cmd.Parameters.AddWithValue("@current_time_utc", DateTime.UtcNow);
                    var returnParam = new SqlParameter("ReturnValue", SqlDbType.Int, 4)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParam);

                    var roles = new List<string>();
                    try
                    {
                        var reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                        while (reader.Read())
                        {
                            roles.Add(reader.GetString(0));
                        }
                        resultValue = (int)returnParam.Value;
                        return roles.ToArray();
                    }
                    catch (SqlException e)
                    {
                        cmd.AddDetailsToException(e);
                        throw;
                    }
                }
            }
        }
    }
}
