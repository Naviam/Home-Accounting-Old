﻿using System;
using System.Data.SqlClient;
using System.Data;
using Naviam.Data;

namespace Naviam.DAL
{
    public class MembershipDataAdapter
    {
        public static UserProfile GetUser(string userName, string password)
        {
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "user_get";

                    cmd.Parameters.AddWithValue("@email", userName);
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new UserProfile(reader);
                            }
                        }
                    }
                    catch (SqlException e)
                    {
                        cmd.AddDetailsToException(e);
                        throw;
                    }
                }
            }
            return null;
        }

        public static int CreateUser(UserProfile entity)
        {
            var res = -1;
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                var commName = "user_create";
                var cmd = holder.Connection.CreateSPCommand(commName);
                try
                {
                    cmd.AddEntityParameters(entity, DbActionType.Insert);
                    cmd.ExecuteNonQuery();
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

        public static int ChangePassword(string applicationName, string email, 
            string newPassword, string passwordSalt, int passwordFormat)
        {
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "procedurename";

                    cmd.Parameters.AddWithValue("@ApplicationName", applicationName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@NewPassword", newPassword);
                    cmd.Parameters.AddWithValue("@PasswordSalt", passwordSalt);
                    cmd.Parameters.AddWithValue("@PasswordFormat", passwordFormat);
                    cmd.Parameters.AddWithValue("@CurrentTimeUtc", DateTime.UtcNow);
                    var parameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(parameter);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException e)
                    {
                        cmd.AddDetailsToException(e);
                        throw;
                    }

                    return (parameter.Value != null) ? ((int)parameter.Value) : -1;
                }
            }
        }
    }
}
