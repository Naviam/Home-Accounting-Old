﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Naviam.Data
{
    [Serializable]
    public class AccountType : DbEntity
    {
        public AccountType() { }
        public AccountType(SqlDataReader reader)
        {
            Id = reader["id"] as int?;
            LanguageId = reader["id_language"] as int?;
            TypeName = reader["type_name"] as string;
            TypeDescription = reader["type_description"] as string;

        }

        public int? LanguageId { get; set; }
        public string TypeName { get; set; }
        public string TypeDescription { get; set; }
    }
}
