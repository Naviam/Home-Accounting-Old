using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naviam.Data;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;

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

        public static List<DateTime> GetRateAbsentDates(int daysCount, int countryId)
        {
            List<DateTime> res = new List<DateTime>();
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
                            {
                                DateTime? date = reader["date"] as DateTime?;
                                if (date.HasValue) res.Add(date.Value);
                            }
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

        public static void BulkUpdateRates(List<CurrRate> rates)
        {
            DataTable ratesTable = Rate.InitDataTable();
            foreach (CurrRate rate in rates)
            {
                ratesTable.Rows.Add(0, rate.CountryId, rate.RateVal, rate.Date, int.Parse(rate.CurrCode));
            }
            //TODO: make additional technical connection with more power rights then web_access but not like sa
            string connectionString = @"Data Source=minsk.servicechannel.com\naviam;Initial Catalog=naviam;Integrated Security=False;User ID=sa;Password=dev_access1;Enlist=False";
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString))
            {
                // The table I'm loading the data to
                bulkCopy.DestinationTableName = "rates";
                // How many records to send to the database in one go (all of them)
                bulkCopy.BatchSize = ratesTable.Rows.Count;
                // Load the data to the database
                try
                {
                    bulkCopy.WriteToServer(ratesTable);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                // Close up          
                bulkCopy.Close();
            }
        }
    }

    [Serializable]
    public class CurrRate
    {
        public CurrRate()
        {

        }
        public CurrRate(DateTime date, string currCode, decimal rateVal, int countryId)
        {

            Date = date;
            CurrCode = currCode;
            RateVal = rateVal;
            CountryId = countryId;
        }

        public DateTime Date { get; set; }
        public string CurrCode { get; set; }
        public decimal RateVal { get; set; }
        public int CountryId { get; set; }
    }
}
