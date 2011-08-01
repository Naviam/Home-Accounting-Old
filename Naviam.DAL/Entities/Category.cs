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
            Subitems = new List<Category>();
        }

        public string Name { get; set; }
        public List<Category> Subitems { get; set; }
        public int? ParentId { get; set; }
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
            foreach (Category item in categories.FindAll(x => x.ParentId == null))
            {
                item.Subitems.AddRange(categories.FindAll(x => x.ParentId == item.Id));
                categories.RemoveAll(x => x.ParentId == item.Id);
            }
            return categories;
        }
    }
}
