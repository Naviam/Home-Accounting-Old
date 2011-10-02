using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    public class CurrenciesDataAdapter
    {

        public static List<Currency> GetCurrencies()
        {
            List<Currency> res = new List<Currency>();
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("web.currencies_get"))
                {
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                res.Add(new Currency(reader));
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

        public static Currency GetCurrencyByShortName(string shortName)
        {
            Currency res = new Currency();
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("web.currencies_get"))
                {
                    cmd.Parameters.AddWithValue("@name_short", shortName.ToDbValue());
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                res = new Currency(reader);
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
