using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace Naviam.Data
{
 
    public static class ArrayExtensions
    {
        public static string ToXml(this string[] input)
        {
            StringWriter result = new StringWriter();
            using (XmlWriter xmlWriter = new XmlTextWriter(result))
            {
                try
                {
                    new XmlSerializer(input.GetType()).Serialize(xmlWriter, input);
                }
                catch (Exception)
                {
                }
            }
            return result.ToString();
        }

        public static string ToXml(this int[] input)
        {
            StringWriter result = new StringWriter();
            using (XmlWriter xmlWriter = new XmlTextWriter(result))
            {
                try
                {
                    new XmlSerializer(input.GetType()).Serialize(xmlWriter, input);
                }
                catch (Exception)
                {
                }
            }
            return result.ToString();
        }
    }

    /// <summary>
    /// Encapsulates extensions related to database values read/write
    /// </summary>
    public static class DbValueExtension
    {
        #region Conversions from C# to DbValue (null=>DbNull)

        
        /// <summary>
        /// Returns a trimmed string, if it is not empty; otherwise, returns null instead of an empty string.
        /// </summary>
        /// <param name="stringToPrepare"></param>
        /// <returns></returns>
        public static object ToDbValue(this string stringToPrepare)
        {
            if (null == stringToPrepare) return DBNull.Value;//null;
            string dbstring = stringToPrepare.Trim();
            return ("" != dbstring) ? (object)dbstring : DBNull.Value;//null;
        }

        /// <summary>
        /// If not null, returns a boxed value; otherwise, returns DbNull value.
        /// </summary>
        /// <param name="valueToConvert"></param>
        /// <returns>Boxed value or DbNull.</returns>
        public static object ToDbValue(this bool? valueToConvert) 
        { return (valueToConvert.HasValue) ? (object)valueToConvert : DBNull.Value; }

        /// <summary>
        /// If not null and does not equal default value, returns a boxed value; otherwise, returns DbNull value.
        /// </summary>
        /// <param name="valueToConvert"></param>
        /// <returns>Boxed value or DbNull.</returns>
        public static object ToDbValue(this char? valueToConvert) 
        { return valueToConvert.HasValue && (default(char) != valueToConvert.Value) ? (object)valueToConvert : DBNull.Value; }

        /// <summary>
        /// If not null and does not equal default value, returns a boxed value; otherwise, returns DbNull value.
        /// </summary>
        /// <param name="valueToConvert"></param>
        /// <returns>Boxed value or DbNull.</returns>
        public static object ToDbValue(this double? valueToConvert)
        { return valueToConvert.HasValue && (default(double) != valueToConvert.Value) ? (object)valueToConvert : DBNull.Value; }

        /// <summary>
        /// If not null and does not equal default value, returns a boxed value; otherwise, returns DbNull value.
        /// </summary>
        /// <param name="valueToConvert"></param>
        /// <returns>Boxed value or DbNull.</returns>
        public static object ToDbValue(this float? valueToConvert)
        { return valueToConvert.HasValue && (default(float) != valueToConvert.Value) ? (object)valueToConvert : DBNull.Value; }

        /// <summary>
        /// If not null and does not equal default value, returns a boxed value; otherwise, returns DbNull value.
        /// </summary>
        /// <param name="valueToConvert"></param>
        /// <returns>Boxed value or DbNull.</returns>
        public static object ToDbValue(this int? valueToConvert)
        { return valueToConvert.HasValue && (default(int) != valueToConvert.Value) ? (object)valueToConvert : DBNull.Value; }

        /// <summary>
        /// If not null and does not equal default value, returns a boxed value; otherwise, returns DbNull value.
        /// </summary>
        /// <param name="valueToConvert"></param>
        /// <returns>Boxed value or DbNull.</returns>
        public static object ToDbValue(this long? valueToConvert)
        { return valueToConvert.HasValue && (default(long) != valueToConvert.Value) ? (object)valueToConvert : DBNull.Value; }

        #endregion

        #region Conversions from DbValues to C#

        public static bool? ToNullableBool(this string stringToNullable)
        {
            if (null == stringToNullable)
            {
                return null;
            }
            bool result;
            return (Boolean.TryParse(stringToNullable.Trim(), out result)) ? new Nullable<bool>(result) : null;
        }

        public static bool FromNullableBool(this object valueFromNullable)
        {
            if (null == valueFromNullable || valueFromNullable is DBNull)
            {
                return false;
            }
            return Convert.ToBoolean(valueFromNullable);
        }

        public static DateTime? ToNullableDateTime(this string stringToNullable)
        {
            if (String.IsNullOrEmpty(stringToNullable))
            {
                return null;
            }
            DateTime result;
            return (DateTime.TryParse(stringToNullable.Trim(), out result)) ? new Nullable<DateTime>(result) : null;
        }

        public static int? ToNullableInt32(this string stringToNullable)
        {
            if (null == stringToNullable)
            {
                return null;
            }
            int result;
            return (Int32.TryParse(stringToNullable.Trim(), out result)) ? new Nullable<int>(result) : null;
        }

        public static long? ToNullableInt64(this string stringToNullable)
        {
            if (null == stringToNullable)
            {
                return null;
            }
            long result;
            return (Int64.TryParse(stringToNullable.Trim(), out result)) ? new Nullable<long>(result) : null;
        }

        public static decimal? ToNullableDecimal(this string stringToNullable)
        {
            if (null == stringToNullable)
            {
                return null;
            }
            decimal result;
            return (Decimal.TryParse(stringToNullable.Trim(), out result)) ? new Nullable<decimal>(result) : null;
        }

        public static int? ToPercentage(this double? value)
        { return ToPercentage(value, 1); }

        public static int? ToPercentage(this double? value, int roundTo)
        {
            return ((null != value) && (null != value))
                ? RoundTo(new int?(Convert.ToInt32(Convert.ToDouble(value) * 100)), roundTo)
                : null;
        }

        public static string ToShortDateString(this object valueToNullable)
        {
            if (null == valueToNullable)
            {
                return "";
            }
            return Convert.ToDateTime(valueToNullable).ToShortDateString();
        }

        #endregion

        public static int? RoundTo(this int? value, int roundTo)
        {
            if (!value.HasValue)
            {
                return null;
            }
            int result = value.Value;
            if (roundTo > 1)
            {
                int mod = result % roundTo;
                result = (mod < roundTo / 2.0) ? result -= mod : result += (roundTo - mod);
            }
            return result;
        }

        #region IDataRecord

        public static int GetIndex(this IDataRecord record, string columnName)
        {
            int result = -1;
            try
            {
                result = record.GetOrdinal(columnName);
            }
            catch (IndexOutOfRangeException)
            {
            }
            return result;
        }

        public static object GetField(this IDataRecord record, string columnName)
        {
            object result = null;
            try
            {
                result = record[columnName];
            }
            catch (IndexOutOfRangeException)
            {
            }
            return result;
        }

        #endregion
    }

    #region IEnumerable

    public static class IEnumerableExtensions
    {
        public delegate string ObjectToText<T>(T item);

        public static string ToText<T>(this IEnumerable<T> list, ObjectToText<T> selector, char separator)
        {
            StringBuilder builder = new StringBuilder();
            foreach (T item in list)
            {
                builder.AppendFormat("{0} {1}", separator, selector(item));
            }
            return (builder.Length > 0) ? builder.Remove(0, 2).ToString() : "";
        }
    }

    #endregion

    #region IEnumerable

    public static class BooleanExtensions
    {

        public static bool HasTrue(this bool? val)
        {
            return val.HasValue && val.Value;
        }
    }

    #endregion

    #region SqlConnectionExtensions

    public static partial class SqlConnectionExtensions
    {
        public static SqlCommand CreateSPCommand(this SqlConnection connection, string spName)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = spName;
            return cmd;
        }
    }

    #endregion

    #region SqlCommandExtensions
    
    //public static partial class SqlCommandExtensions
    //{
    //    public static void AddDetailsToException(this SqlCommand command, SqlException e)
    //    {
    //        string procedureName = String.Format("Procedure Name: {0}", command.CommandText);
    //        e.Data[Global.ProcNameExceptionDataKey] = procedureName;
    //        string parameters = String.Format("Passed parameters: {0}", Environment.NewLine);
    //        foreach (SqlParameter sqlParam in command.Parameters)
    //        {
    //            string paramValue = sqlParam.Value != null ? sqlParam.Value.ToString() : "";
    //            parameters = parameters + sqlParam.ParameterName + ": " + paramValue +
    //                "(" + sqlParam.Size.ToString() + "), ";
    //        }
    //        e.Data[Global.ParamExceptionDataKey] = parameters.Remove(parameters.Length - 2);
    //    }
    //}

    #endregion

}
