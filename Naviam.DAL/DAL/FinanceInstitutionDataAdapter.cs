﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naviam.Data;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    public class FinanceInstitutionDataAdapter
    {
       
        public static List<FinanceInstitution> Get()
        {
            List<FinanceInstitution> res = new List<FinanceInstitution>();
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("fininst_get"))
                {
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                res.Add(new FinanceInstitution(reader));
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

        public static List<FinanceInstitutionLinkToAccount> GetLinksToAccount()
        {
            List<FinanceInstitutionLinkToAccount> res = new List<FinanceInstitutionLinkToAccount>();
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("mapping_fininst_to_account_get"))
                {
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                res.Add(new FinanceInstitutionLinkToAccount(reader));
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
