using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Web.Hosting;
using Naviam.DAL;

namespace Naviam.Domain.Entities
{
    internal static class SecUtility
    {
        // Methods
        internal static void CheckArrayParameter(ref string[] param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
            if (param.Length < 1)
            {
                throw new ArgumentException(String.Empty, paramName);
            }
            var hashtable = new Hashtable(param.Length);
            for (var i = param.Length - 1; i >= 0; i--)
            {
                CheckParameter(ref param[i], checkForNull, checkIfEmpty, checkForCommas, maxSize, paramName + "[ " + i.ToString(CultureInfo.InvariantCulture) + " ]");
                if (hashtable.Contains(param[i]))
                {
                    throw new ArgumentException(String.Empty, paramName);
                }
                hashtable.Add(param[i], param[i]);
            }
        }

        internal static void CheckParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName)
        {
            if (param == null)
            {
                if (checkForNull)
                {
                    throw new ArgumentNullException(paramName);
                }
            }
            else
            {
                param = param.Trim();
                if (checkIfEmpty && (param.Length < 1))
                {
                    throw new ArgumentException("Parameter cannot be empty", paramName);
                }
                if ((maxSize > 0) && (param.Length > maxSize))
                {
                    throw new ArgumentException("Parameter too long", paramName);
                }
                if (checkForCommas && param.Contains(","))
                {
                    throw new ArgumentException("Parameter cannot contain comma", paramName);
                }
            }
        }

        internal static void CheckPasswordParameter(ref string param, int maxSize, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
            if (param.Length < 1)
            {
                throw new ArgumentException("Parameter cannot be empty", paramName);
            }
            if ((maxSize > 0) && (param.Length > maxSize))
            {
                throw new ArgumentException("Parameter too long", paramName);
            }
        }

        internal static void CheckSchemaVersion(ProviderBase provider, SqlConnection connection, string[] features, string version, ref int schemaVersionCheck)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (features == null)
            {
                throw new ArgumentNullException("features");
            }
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            if (schemaVersionCheck == -1)
            {
                throw new ProviderException("Provider schema version not match");
            }
            if (schemaVersionCheck == 0)
            {
                lock (provider)
                {
                    if (schemaVersionCheck == -1)
                    {
                        throw new ProviderException("Provider schema version not match");
                    }
                    if (schemaVersionCheck == 0)
                    {
                        foreach (var str in features)
                        {
                            var command = new SqlCommand("dbo.aspnet_CheckSchemaVersion", connection)
                                                     {
                                                         CommandType = CommandType.StoredProcedure
                                                     };
                            var parameter = new SqlParameter("@Feature", str);
                            command.Parameters.Add(parameter);
                            parameter = new SqlParameter("@CompatibleSchemaVersion", version);
                            command.Parameters.Add(parameter);
                            parameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
                                            {
                                                Direction = ParameterDirection.ReturnValue
                                            };
                            command.Parameters.Add(parameter);
                            command.ExecuteNonQuery();
                            if (((parameter.Value != null) ? ((int)parameter.Value) : -1) != 0)
                            {
                                schemaVersionCheck = -1;
                                throw new ProviderException("Provider schema version not match");
                            }
                        }
                        schemaVersionCheck = 1;
                    }
                }
            }
        }

        internal static bool GetBooleanValue(NameValueCollection config, string valueName, bool defaultValue)
        {
            bool flag;
            string str = config[valueName];
            if (str == null)
            {
                return defaultValue;
            }
            if (!bool.TryParse(str, out flag))
            {
                throw new ProviderException("Value must be boolean");
            }
            return flag;
        }

        internal static string GetConnectionString(NameValueCollection config)
        {
            string str = config["connectionString"];
            if (string.IsNullOrEmpty(str))
            {
                string str2 = config["connectionStringName"];
                if (string.IsNullOrEmpty(str2))
                {
                    throw new ProviderException("Connection name not specified");
                }
                str = SqlConnectionHelper.GetConnString();
                if (string.IsNullOrEmpty(str))
                {
                    throw new ProviderException("Connection string not found");
                }
            }
            return str;
        }

        internal static string GetDefaultAppName()
        {
            try
            {
                string applicationVirtualPath = HostingEnvironment.ApplicationVirtualPath;
                if (string.IsNullOrEmpty(applicationVirtualPath))
                {
                    applicationVirtualPath = Process.GetCurrentProcess().MainModule.ModuleName;
                    int index = applicationVirtualPath.IndexOf('.');
                    if (index != -1)
                    {
                        applicationVirtualPath = applicationVirtualPath.Remove(index);
                    }
                }
                if (string.IsNullOrEmpty(applicationVirtualPath))
                {
                    return "/";
                }
                return applicationVirtualPath;
            }
            catch
            {
                return "/";
            }
        }

        internal static int GetIntValue(NameValueCollection config, string valueName, int defaultValue, bool zeroAllowed, int maxValueAllowed)
        {
            int num;
            string s = config[valueName];
            if (s == null)
            {
                return defaultValue;
            }
            if (!int.TryParse(s, out num))
            {
                if (zeroAllowed)
                {
                    throw new ProviderException("Value must be non negative integer");
                }
                throw new ProviderException("Value must be positive integer");
            }
            if (zeroAllowed && (num < 0))
            {
                throw new ProviderException("Value must be non negative integer");
            }
            if (!zeroAllowed && (num <= 0))
            {
                throw new ProviderException("Value must be positive integer");
            }
            if ((maxValueAllowed > 0) && (num > maxValueAllowed))
            {
                throw new ProviderException("Value too big");
            }
            return num;
        }

        internal static bool ValidateParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize)
        {
            if (param == null)
            {
                return !checkForNull;
            }
            param = param.Trim();
            return (((!checkIfEmpty || (param.Length >= 1)) && ((maxSize <= 0) || (param.Length <= maxSize))) && (!checkForCommas || !param.Contains(",")));
        }

        internal static bool ValidatePasswordParameter(ref string param, int maxSize)
        {
            if (param == null)
            {
                return false;
            }
            if (param.Length < 1)
            {
                return false;
            }
            if ((maxSize > 0) && (param.Length > maxSize))
            {
                return false;
            }
            return true;
        }
    }
}