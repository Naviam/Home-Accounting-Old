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
            Code = reader["code"] as int?;
            NameShort = reader["name_short"] as string;
            Name = reader["name"] as string;
        }

        public string NameShort { get; set; }
        public int? Code { get; set; }
        public string Name { get; set; }
    }
}
