using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Naviam.Data
{
    public enum RuleTypes { Equals = 0, Regex = 1 }

    [Serializable]
    public class FieldRule : DbEntity
    {
        public FieldRule() 
        {
            
        }

        public FieldRule(SqlDataReader reader)
        {
            Id = reader["id"] as int?;
            UserId = reader["id_user"] as int?;
            FildTarget = reader["field_target"] as string;
            FildTargetValue = reader["field_target_value"] as string;
            Fild = reader["field"] as string;
            FildValue = reader["field_value"] as string;
            Priority = reader["priority"] as int?;
            RuleType = reader["type_rule"] as RuleTypes?;
        }

        public int? UserId { get; set; }
        public string FildTarget { get; set; }
        public string FildTargetValue { get; set; }
        public string Fild { get; set; }
        public string FildValue { get; set; }
        public int? Priority { get; set; }
        public RuleTypes? RuleType { get; set; }
    }
}
