using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    public class AccountTypesDataAdapter
    {

        public static List<AccountType> GetAccountTypes() { return GetAccountTypes(false); }
        public static List<AccountType> GetAccountTypes(bool forceSqlLoad)
        {
            List<AccountType> res = new List<AccountType>();
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("web.account_types_get"))
                {
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                res.Add(new AccountType(reader));
                        }
                    }
                    catch (SqlException e)
                    {
                        cmd.AddDetailsToException(e);
                        throw;
                    }
                }

            }
            return res;
        }

    }
}
