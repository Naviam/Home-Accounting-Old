using System.Collections.Generic;
using Naviam.Data;
using System.Data.SqlClient;
using System;
using System.Data;

namespace Naviam.DAL
{
    public class SmsDataAdapter
    {
        public static Account GetAccountBySms(string cardNumber, int? id_modem, int? id_bank)
        {
            Account res = null;
            using (SqlConnectionHolder holder = SqlConnectionHelper.GetConnection(SqlConnectionHelper.ConnectionType.Naviam))
            {
                using (SqlCommand cmd = holder.Connection.CreateSPCommand("sms_account_get"))
                {
                    cmd.Parameters.AddWithValue("@card_number", cardNumber);
                    cmd.Parameters.AddWithValue("@id_modem", id_modem);
                    cmd.Parameters.AddWithValue("@id_bank", id_bank);
                    try
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {

                            reader.Read();
                            if (reader.HasRows)
                            {
                                res = new Account(reader);
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

        public static int? SmsServiceSubscribe(int? id_account, DateTime? date_start, DateTime? date_end)
        {
            int? res = null;
            using (SqlConnectionHolder holder = SqlConnectionHelper.GetConnection(SqlConnectionHelper.ConnectionType.Naviam))
            {
                using (SqlCommand cmd = holder.Connection.CreateSPCommand("sms_subscribe"))
                {
                    cmd.Parameters.AddWithValue("@id_account", id_account.ToDbValue());
                    cmd.Parameters.AddWithValue("@date_start", date_start.ToDbValue());
                    cmd.Parameters.AddWithValue("@date_end", date_end.ToDbValue());
                    cmd.Parameters.Add("@id_modem", SqlDbType.Int).Direction = ParameterDirection.InputOutput;
                    try
                    {
                        cmd.ExecuteNonQuery();
                        res = cmd.GetRowIdParameter("@id_modem");
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
