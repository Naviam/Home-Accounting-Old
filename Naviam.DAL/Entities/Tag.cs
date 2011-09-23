using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace Naviam.Data
{
    [Serializable]
    public class Tag : DbEntity
    {
        public Tag()
        {}

        public Tag(SqlDataReader reader)
        {
            Id = reader["id"] as int?;
            Name = reader["name"] as string;
            UserId = reader["id_user"] as int?;
        }

        public string Name { get; set; }
        public int? UserId { get; set; }
    }

    public static partial class SqlCommandExtensions
    {
        /// <summary>
        /// Appends Tag-specific parameters to the specificied SqlCommand. 
        /// </summary>
        /// <param name="command">SqlCommand to be executed.</param>
        /// <param name="alert">Instance of Tag class</param>
        /// <param name="action">Database action type (select, insert, update, delete).</param>
        public static void AddEntityParameters(this SqlCommand command, Tag entity, DbActionType action)
        {
            command.AddCommonParameters(entity.Id, action);
            command.Parameters.Add("@name", SqlDbType.NVarChar).Value = entity.Name.ToDbValue();
            command.Parameters.Add("@id_user", SqlDbType.Int).Value = entity.UserId.ToDbValue();
        }
    }
}
