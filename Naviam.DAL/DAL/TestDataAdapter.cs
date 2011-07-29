using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    public class TestDataAdapter
    {
        public static void Test()
        {
            using (SqlConnectionHolder holder = SqlConnectionHelper.GetConnection(SqlConnectionHelper.ConnectionType.Naviam))
            {
                SqlCommand command = new SqlCommand("select * from naviUsers", holder.Connection);
            }
        }
    }
}
