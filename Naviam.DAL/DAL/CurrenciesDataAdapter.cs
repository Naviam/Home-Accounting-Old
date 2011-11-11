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
                using (var cmd = holder.Connection.CreateSPCommand("currencies_get"))
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

        public static List<DateTime?> GetRateAbsentDates(int daysCount, int countryId)
        {
            List<DateTime?> res = new List<DateTime?>();
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("rate_absent_dates_get"))
                {
                    cmd.Parameters.AddWithValue("@days_count", daysCount);
                    cmd.Parameters.AddWithValue("@id_country", countryId);
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                res.Add(reader["date"] as DateTime?);
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
