using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Naviam.Data
{
    [Serializable]
    public class Currency: DbEntity
    {
        public Currency(){}
        public Currency(SqlDataReader reader)
        {
            Id = reader["id"] as int?;
            NameShort = reader["name_short"] as string;
        }

        public string NameShort { get; set; }
    }
}
