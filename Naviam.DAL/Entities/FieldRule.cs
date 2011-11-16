using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace Naviam.Data
{
    public enum RuleTypes { Equals = 0, Regex}

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
            short? val = reader["type_rule"] as short?;
            RuleType = val.HasValue ? (RuleTypes)val.Value : 0;
        }

        public int? UserId { get; set; }
        public string FildTarget { get; set; }
        public string FildTargetValue { get; set; }
        public string Fild { get; set; }
        public string FildValue { get; set; }
        public int? Priority { get; set; }
        public RuleTypes RuleType { get; set; }
    }

    public static partial class SqlCommandExtensions
    {
        /// <summary>
        /// Appends Rule-specific parameters to the specificied SqlCommand. 
        /// </summary>
        /// <param name="command">SqlCommand to be executed.</param>
        /// <param name="alert">Instance of FieldRule class</param>
        /// <param name="action">Database action type (select, insert, update, delete).</param>
        public static void AddEntityParameters(this SqlCommand command, FieldRule rule, DbActionType action)
        {
            command.AddCommonParameters(rule.Id, action);
            command.Parameters.Add("@id_user", SqlDbType.Int).Value = rule.UserId.ToDbValue();
            command.Parameters.Add("@field_target", SqlDbType.NVarChar).Value = rule.FildTarget.ToDbValue();
            command.Parameters.Add("@field_target_value", SqlDbType.NVarChar).Value = rule.FildTargetValue.ToDbValue();
            command.Parameters.Add("@field", SqlDbType.NVarChar).Value = rule.Fild.ToDbValue();
            command.Parameters.Add("@field_value", SqlDbType.NVarChar).Value = rule.FildValue.ToDbValue();
            command.Parameters.Add("@priority", SqlDbType.Int).Value = rule.Priority.ToDbValue();
            command.Parameters.Add("@type_rule", SqlDbType.SmallInt).Value = (short)rule.RuleType;
        }
    }
}
