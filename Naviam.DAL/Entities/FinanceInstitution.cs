using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Naviam.Data
{
    [Serializable]
    public class FinanceInstitution : DbEntity
    {
        public string Name { get; set; }
        public int? TypeId { get; set; }
        public string Description { get; set; }
       
        public FinanceInstitution() { }
        public FinanceInstitution(SqlDataReader reader)
        {
            Id = reader["id"] as int?;
            Name = reader["name"] as string;
            Description = reader["description"] as string;
            TypeId = reader["id_type"] as int?;
        }
    }

    [Serializable]
    public class FinanceInstitutionLinkToAccount
    {
        public int? FinanceTypeId { get; set; }
        public int? AccountTypeId { get; set; }
        
        public FinanceInstitutionLinkToAccount() { }
        public FinanceInstitutionLinkToAccount(SqlDataReader reader)
        {
            FinanceTypeId = reader["id_fin_type"] as int?;
            AccountTypeId = reader["id_acc_type"] as int?;
        }
    }

}
