using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.IO;

using Naviam.Data;

using Npgsql;

namespace Naviam.DAL
{
    public class UserDataAdapter
    {
        #region Encryption/Decryption

        // encryption key
        static byte[] HKey = { 214, 46, 220, 83, 160, 73, 11, 39, 56, 140, 19, 202, 3, 11, 191, 178, 56, 37, 98, 12, 34, 88, 22, 77 }; //24
        static byte[] HIV = { 23, 78, 28, 13, 44, 98, 245, 34 }; //8

        static string Encrypt(string input)
        {
            TripleDESCryptoServiceProvider key = new TripleDESCryptoServiceProvider();

            // Create a memory stream.
            MemoryStream ms = new MemoryStream();

            // Create a CryptoStream using the memory stream and the 
            CryptoStream encStream = new CryptoStream(ms, key.CreateEncryptor(HKey, HIV), CryptoStreamMode.Write);

            // Create a StreamWriter to write a string
            // to the stream.
            StreamWriter sw = new StreamWriter(encStream);

            // Write the plaintext to the stream.
            sw.WriteLine(input);

            // Close the StreamWriter and CryptoStream.
            sw.Close();
            encStream.Close();

            // Get an array of bytes that represents
            // the memory stream.
            byte[] buffer = ms.ToArray();

            // Close the memory stream.
            ms.Close();

            string result = "";
            //Convert to string
            for (int i = 0; i < buffer.Length; i++)
            {
                result += buffer[i].ToString("X2");
            }
            // Return the encrypted string.
            return result;
        }

        static string Decrypt(string input)
        {
            TripleDESCryptoServiceProvider key = new TripleDESCryptoServiceProvider();
            byte[] buffer = new byte[input.Length / 2];
            for (int i = 0; i < input.Length; i += 2)
            {
                buffer[i / 2] = Convert.ToByte(input.Substring(i, 2), 16);
            }
            // Create a memory stream to the passed buffer.
            MemoryStream ms = new MemoryStream(buffer);

            // Create a CryptoStream using the memory stream and the 
            CryptoStream encStream = new CryptoStream(ms, key.CreateDecryptor(HKey, HIV), CryptoStreamMode.Read);

            // Create a StreamReader for reading the stream.
            StreamReader sr = new StreamReader(encStream);

            // Read the stream as a string.
            string val = sr.ReadLine();

            // Close the streams.
            sr.Close();
            encStream.Close();
            ms.Close();

            return val;
        }
        #endregion Encryption/Dec

        private static string HashPassword(string userName, string password)
        {
            //byte[] plainTextBytes = Encoding.UTF8.GetBytes(userName);
            //byte[] saltBytes = new byte[plainTextBytes.Length];
            //for (int i = 0; i < plainTextBytes.Length; i++)
            //    saltBytes[i] = plainTextBytes[i];
            return SimpleHash.ComputeHash(userName + password + "SCEX", "SHA512", null);
        }

        public static UserProfile GetUserProfile(string userName, string password)
        {
            //string tst = HashPassword("s@s.s", "1");
            UserProfile res = null;
            using (SqlConnectionHolder holder = SqlConnectionHelper.GetConnection(SqlConnectionHelper.ConnectionType.Naviam))
            {
                NpgsqlCommand command = new NpgsqlCommand("\"GetUser\"", holder.Connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add("userName", NpgsqlTypes.NpgsqlDbType.Name, 64).Value = userName;
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        res = new UserProfile(reader);
                        if (!SimpleHash.VerifyHash(userName + password + "SCEX", "SHA512", res.Password))
                            res = null;
                    }
                }
                return res;
            }
        }
    }
}
