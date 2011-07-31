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
         //TODO: read fields
        }

        public string Name { get; set; }
        public List<Category> Subitems { get; set; }
    }
}
