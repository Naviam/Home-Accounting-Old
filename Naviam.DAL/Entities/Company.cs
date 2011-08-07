using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Naviam.Data
{
    public class Company: DbEntity
    {
        public Company()  {}

        public Company(IDataRecord record)
        {
            Id = record["id"] as int?;
            CountryId = record["id_country"] as int?;
            BuinessName = record["name_business"] as string;
            TypeId = record["id_company_type"] as int?;
        }

        public int? CountryId { get; set; }
        public string BuinessName { get; set; }
        public int? TypeId { get; set; }
    }
}
