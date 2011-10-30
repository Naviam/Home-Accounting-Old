using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Naviam.Data
{
    public class CategoryMerchant
    {
        public CategoryMerchant()
        {
        }
        public CategoryMerchant(string merchant, int categoryId)
        {
            Merchant = merchant;
            CategoryId = categoryId;
        }
        public CategoryMerchant(SqlDataReader reader)
        {
            Merchant = reader["merchant"] as string;
            CategoryId = (reader["id_category"] as int?).Value;
        }

        public int? CategoryId { get; set; }
        public string Merchant { get; set; }
    }
}
