using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using System.Data.SqlClient;

namespace Naviam.Data
{

    [Serializable]
    public class Category : DbEntity
    {
        public Category() 
        {
            Subitems = new List<Category>();
        }
        public Category(SqlDataReader reader) 
        {
            Id = reader["id"] as int?;
            ParentId = reader["parent_id"] as int?;
            Name = reader["name"] as string;
            UserId = reader["id_user"] as int?;
            Subitems = new List<Category>();
        }

        public string Name { get; set; }
        public List<Category> Subitems { get; set; }
        public int? ParentId { get; set; }
        public int? UserId { get; set; }
    }
    
    [Serializable]
    public class Categories : List<Category>
    {
        public Categories() { }
        public Categories(SqlDataReader reader) 
        {
            while (reader.Read())
            {
                Add(new Category(reader));
            }
        }

        public static List<Category> GetTree(List<Category> categories)
        {
            var res = new List<Category>();
            foreach (Category item in categories.FindAll(x => x.ParentId == null))
            {
                res.Add(item);
                item.Subitems.Clear();
                item.Subitems.AddRange(categories.FindAll(x => x.ParentId == item.Id));
                //categories.RemoveAll(x => x.ParentId == item.Id);
            }
            return res;
        }
    }

    public static partial class SqlCommandExtensions
    {
        /// <summary>
        /// Appends Category-specific parameters to the specificied SqlCommand. 
        /// </summary>
        /// <param name="command">SqlCommand to be executed.</param>
        /// <param name="alert">Instance of Category class</param>
        /// <param name="action">Database action type (select, insert, update, delete).</param>
        public static void AddEntityParameters(this SqlCommand command, Category entity, DbActionType action)
        {
            command.AddCommonParameters(entity.Id, action);
            command.Parameters.Add("@id_user", SqlDbType.Int).Value = entity.UserId.ToDbValue();
            command.Parameters.Add("@parent_id", SqlDbType.Int).Value = entity.ParentId.ToDbValue();
            command.Parameters.Add("@name", SqlDbType.NVarChar).Value = entity.Name.ToDbValue();
        }
    }
}
