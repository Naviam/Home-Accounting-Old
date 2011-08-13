#define _USE_MS_SQL

using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.Data;
using System.Web.Hosting;

namespace Naviam.DAL
{
    public class Global
    {
        public static string ProcNameExceptionDataKey = "procname";
        public static string ParamExceptionDataKey = "parameters";
    }

    #region SqlCommandExtensions

    public static class SqlCommandExtensions
    {
        public static void AddDetailsToException(this SqlCommand command, SqlException e)
        {
            var procedureName = String.Format("Procedure Name: {0}", command.CommandText);
            e.Data[Global.ProcNameExceptionDataKey] = procedureName;
            var parameters = String.Format("Passed parameters: {0}", Environment.NewLine);
            foreach (SqlParameter sqlParam in command.Parameters)
            {
                var paramValue = sqlParam.Value != null ? sqlParam.Value.ToString() : "";
                parameters = parameters + sqlParam.ParameterName + ": " + paramValue +
                    "(" + sqlParam.Size.ToString() + "), ";
            }
            e.Data[Global.ParamExceptionDataKey] = parameters.Remove(parameters.Length - 2);
        }
    }

    #endregion

    /// <summary>
    /// Provides access to SqlConnection Helper Methods
    /// </summary>
    public static class SqlConnectionHelper
    {
        public enum ConnectionType { Naviam }

        const string KeyDataSource = "connectionString.dataSource";

        private static readonly Hashtable ConnectionStrings = new Hashtable();
        private static readonly string DataSource = ConfigurationManager.AppSettings[KeyDataSource];

        #region METHOD::GetConnection
        public static SqlConnectionHolder GetConnection(ConnectionType connectionType = ConnectionType.Naviam)
        {
            return GetConnection(GetConnString(connectionType), false);
        }
        public static SqlConnectionHolder GetConnection(string connectionString, bool revertImpersonation)
        {
            var holder = new SqlConnectionHolder(connectionString);
            var closeConn = true;
            try
            {
                holder.Open(revertImpersonation);
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
        public static string GetConnString(ConnectionType connectionType = ConnectionType.Naviam)
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
        private const string SqlErrorConnectionString = "An error occurred while attempting to initialize a System.Data.SqlClient.SqlConnection object. The value that was provided for the connection string may be wrong, or it may contain an invalid syntax.";

        private bool _opened;
        
        public SqlConnection Connection { get; private set; }

        internal SqlConnectionHolder(string connectionString)
        {
            try
            {
                Connection = new SqlConnection(connectionString);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(SqlErrorConnectionString, "connectionString", e);
            }
        }
        
        public void Open(bool revertImpersonate)
        {
            if (_opened)
                return; // Already opened

            if (revertImpersonate)
            {
                using (HostingEnvironment.Impersonate())
                {
                    Connection.Open();
                }
            }
            else
            {
                Connection.Open();
            }

            _opened = true; // Open worked!
        }

        public void Close()
        {
            if (!_opened) // Not open!
                return;
            // Close connection
            Connection.Close();
            _opened = false;
        }

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