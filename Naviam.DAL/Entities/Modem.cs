using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace Naviam.Data
{
    [Serializable]
    public class Modem : DbEntity
    {
        public Modem()
        {

        }
        public Modem(SqlDataReader reader)
        {
            Id = reader["id"] as int?;
            Description = reader["description"] as string;
            Gateway = reader["gateway"] as string;
        }

        public string Description { get; set; }
        public string Gateway { get; set; }
    }//calass Modem

    public static partial class SqlCommandExtensions
    {
        /// <summary>
        /// Appends Account-specific parameters to the specificied SqlCommand. 
        /// </summary>
        /// <param name="command">SqlCommand to be executed.</param>
        /// <param name="alert">Instance of Modem class</param>
        /// <param name="action">Database action type (select, insert, update, delete).</param>
        public static void AddEntityParameters(this SqlCommand command, Modem account, DbActionType action)
        {
            command.AddCommonParameters(account.Id, action);
            command.Parameters.Add("@description", SqlDbType.NVarChar).Value = account.Description.ToDbValue();
            command.Parameters.Add("@gateway", SqlDbType.NVarChar).Value = account.Description.ToDbValue();
        }
    }

}
