using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Npgsql;

namespace Naviam.DAL
{
    public class TestDataAdapter
    {
        public static void Test()
        {
            using (SqlConnectionHolder holder = SqlConnectionHelper.GetConnection(SqlConnectionHelper.ConnectionType.Naviam))
            {
                NpgsqlCommand command = new NpgsqlCommand("select * from tst", holder.Connection);
            }

        }
    }
}
