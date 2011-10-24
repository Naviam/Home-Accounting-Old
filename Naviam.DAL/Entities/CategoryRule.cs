using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Naviam.Data
{
    public class CategoryRule: DbEntity
    {
        public CategoryRule()
        {

        }
        public CategoryRule(SqlDataReader reader)
        {
            Id = reader["id"] as int?;
            CategoryId = reader["category_id"] as int?;
            Priority= reader["priority"] as int?;
            RegX = reader["regex"] as string;
        }

        public int? CategoryId { get; set; }
        public int? Priority { get; set; }
        public string RegX { get; set; }
    }
}
