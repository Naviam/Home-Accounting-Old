using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Naviam.Data
{
    [Serializable]
    public class Rate : DbEntity
    {
        public Rate() { }
        public Rate(SqlDataReader reader)
        {
            Id = reader["id"] as int?;
            CountryId = reader["id_country"] as int?;
            Value = reader["value"] as decimal?;
            Date = reader["date"] as DateTime?;
            CurrencyId = reader["id_currency"] as int?;
        }

        public string NameShort { get; set; }
        public int? CountryId { get; set; }
        public decimal? Value { get; set; }
        public DateTime? Date { get; set; }
        public int? CurrencyId { get; set; }
    }
}
