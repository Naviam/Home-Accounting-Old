#define _USE_MS_SQL

using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.Data;
using Npgsql;

namespace Naviam.DAL
{
    /// <summary>
    /// Provides access to SqlConnection Helper Methods
    /// </summary>
    public static class SqlConnectionHelper
    {
        public enum ConnectionType
        {
            Naviam
        }

        const string KeyDataSource = "connectionString.dataSource";

        private static readonly Hashtable ConnectionStrings = new Hashtable();
        private static readonly string DataSource = ConfigurationManager.AppSettings[KeyDataSource];

        #region METHOD::GetConnection
        public static SqlConnectionHolder GetConnection(ConnectionType connectionType)
        {
            return GetConnection(GetConnString(connectionType));
        }
        public static SqlConnectionHolder GetConnection(string connectionString)
        {
            var holder = new SqlConnectionHolder(connectionString);
            var closeConn = true;
            try
            {
                holder.Open();
                closeConn = false;
            }
            finally
            {
                if (closeConn)
                {
                    holder.Close();
                    holder = null;
                }
            }
            return holder;
        }
        #endregion

        #region METHOD::GetConnString

        public static string GetConnString(ConnectionType connectionType)
        {
            if (ConnectionStrings[connectionType] == null)
            {
                string connString = null;
                var connSection = ConfigurationManager.ConnectionStrings[connectionType.ToString()];
                if (connSection != null)
                    connString = connSection.ConnectionString;
                if (!String.IsNullOrEmpty(DataSource) && connString != null && connString.Contains("{0}"))
                    connString = String.Format(connString, DataSource);
                ConnectionStrings[connectionType] = connString;

            }
            return ConnectionStrings[connectionType] as string;
        }
        #endregion
    }

    /// <summary>
    /// Manages connection pooling
    /// </summary>
    public sealed class SqlConnectionHolder : IDisposable
    {
        private const string SqlErrorConnectionString = "An error occurred while attempting to initialize a SqlConnectionHolder object. The value that was provided for the connection string may be wrong, or it may contain an invalid syntax.";

        private bool _opened;

        #region PROPERTY::Connection

#if _USE_MS_SQL 
        public SqlConnection Connection { get; private set; }
#else
        public NpgsqlConnection Connection { get; private set; }
#endif

        #endregion

        #region Constructor
        internal SqlConnectionHolder(string connectionString)
        {
            try
            {
#if _USE_MS_SQL 
                Connection = new SqlConnection(connectionString);
#else
                Connection = new NpgsqlConnection(connectionString);
#endif
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(SqlErrorConnectionString, "connectionString", e);
            }
        }
        #endregion

        #region METHOD::Open
        internal void Open()
        {
            if (_opened)
                return; // Already opened

            Connection.Open();

            _opened = true; // Open worked!
        }
        #endregion

        #region METHOD::Close
        internal void Close()
        {
            if (!_opened) // Not open!
                return;
            // Close connection
            Connection.Close();
            _opened = false;
        }
        #endregion

        public void Dispose()
        {
            if (Connection == null) return;
            if (Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
            Connection = null;
        }
    }


}