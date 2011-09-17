using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    public class TagsDataAdapter
    {
        public static List<Tag> GetTags(int? userId)
        {
            List<Tag> res = new List<Tag>();
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("tags_get"))
                {
                    cmd.Parameters.AddWithValue("@id_user", userId.ToDbValue());
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                res.Add(new Tag(reader));
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

        public static int InsertUpdate(Tag entity, int? userId, DbActionType action)
        {
            var res = -1;
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                var commName = action == DbActionType.Insert ? "tag_create" : "tag_update";
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
                using (var cmd = holder.Connection.CreateSPCommand("tag_delete"))
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
    }

}
