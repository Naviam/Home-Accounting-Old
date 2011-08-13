using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Data.SqlClient;
using Naviam.DAL;
using System.Text;
using Naviam.Domain.Entities;

namespace Naviam.Domain.Concrete.Providers
{
    public class NaviamMembershipProvider : MembershipProvider
    {
        // Fields
        private string _appName;
        private bool _enablePasswordReset;
        private bool _enablePasswordRetrieval;
        private MembershipPasswordCompatibilityMode _legacyPasswordCompatibilityMode;
        private int _maxInvalidPasswordAttempts;
        private int _minRequiredNonalphanumericCharacters;
        private int _minRequiredPasswordLength;
        private int _passwordAttemptWindow;
        private MembershipPasswordFormat _passwordFormat;
        private string _passwordStrengthRegularExpression;
        private bool _requiresQuestionAndAnswer;
        private bool _requiresUniqueEmail;
        private int _schemaVersionCheck;
        private string _sqlConnectionString;
        private const int PasswordSize = 14;
        private string _sHashAlgorithm;
        private const int SaltSize = 0x10;

        // Methods
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            int num;
            SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");
            SecUtility.CheckParameter(ref oldPassword, true, true, false, 0x80, "oldPassword");
            SecUtility.CheckParameter(ref newPassword, true, true, false, 0x80, "newPassword");
            string salt;
            if (!CheckPassword(username, oldPassword, false, false, out salt, out num))
            {
                return false;
            }
            if (newPassword.Length < MinRequiredPasswordLength)
            {
                throw new ArgumentException("Password too short", "newPassword");
            }
            var num3 = newPassword.Where((t, i) => !char.IsLetterOrDigit(newPassword, i)).Count();
            if (num3 < MinRequiredNonAlphanumericCharacters)
            {
                throw new ArgumentException("Password need more non alpha numeric chars", "newPassword");
            }
            if ((PasswordStrengthRegularExpression.Length > 0) && !Regex.IsMatch(newPassword, PasswordStrengthRegularExpression))
            {
                throw new ArgumentException("Password does not match regular expression", "newPassword");
            }
            var objValue = EncodePassword(newPassword, num, salt);
            if (objValue.Length > 0x80)
            {
                throw new ArgumentException("Membership password too long", "newPassword");
            }
            var e = new ValidatePasswordEventArgs(username, newPassword, false);
            OnValidatingPassword(e);
            if (e.Cancel)
            {
                if (e.FailureInformation != null)
                {
                    throw e.FailureInformation;
                }
                throw new ArgumentException("Membership Custom Password Validation Failure", "newPassword");
            }
            var status = MembershipDataAdapter.ChangePassword(ApplicationName, username, objValue, salt, num);
            if (status != 0)
            {
                var exceptionText = GetExceptionText(status);
                if (IsStatusDueToBadPassword(status))
                {
                    throw new MembershipPasswordException(exceptionText);
                }
                throw new ProviderException(exceptionText);
            }
            return true;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            string str;
            int num;
            bool flag;
            SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");
            SecUtility.CheckParameter(ref password, true, true, false, 0x80, "password");
            if (!CheckPassword(username, password, false, false, out str, out num))
            {
                return false;
            }
            SecUtility.CheckParameter(ref newPasswordQuestion, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 0x100, "newPasswordQuestion");
            if (newPasswordAnswer != null)
            {
                newPasswordAnswer = newPasswordAnswer.Trim();
            }
            SecUtility.CheckParameter(ref newPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 0x80, "newPasswordAnswer");
            var str2 = !string.IsNullOrEmpty(newPasswordAnswer) ? EncodePassword(newPasswordAnswer.ToLower(CultureInfo.InvariantCulture), num, str) : newPasswordAnswer;
            SecUtility.CheckParameter(ref str2, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 0x80, "newPasswordAnswer");
            try
            {
                SqlConnectionHolder connection = null;
                try
                {
                    connection = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion(connection.Connection);
                    var command = new SqlCommand("dbo.aspnet_Membership_ChangePasswordQuestionAndAnswer", connection.Connection)
                    {
                        CommandTimeout = CommandTimeout,
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    command.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                    command.Parameters.Add(CreateInputParam("@NewPasswordQuestion", SqlDbType.NVarChar, newPasswordQuestion));
                    command.Parameters.Add(CreateInputParam("@NewPasswordAnswer", SqlDbType.NVarChar, str2));
                    var parameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    command.Parameters.Add(parameter);
                    command.ExecuteNonQuery();
                    var status = (parameter.Value != null) ? ((int)parameter.Value) : -1;
                    if (status != 0)
                    {
                        throw new ProviderException(GetExceptionText(status));
                    }
                    flag = status == 0;
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
            return flag;
        }

        private bool CheckPassword(string username, string password, bool updateLastLoginActivityDate, bool failIfNotApproved)
        {
            string str;
            int num;
            return CheckPassword(username, password, updateLastLoginActivityDate, failIfNotApproved, out str, out num);
        }

        private bool CheckPassword(string username, string password, bool updateLastLoginActivityDate, bool failIfNotApproved, out string salt, out int passwordFormat)
        {
            SqlConnectionHolder connection = null;
            string str;
            int num;
            int num2;
            int num3;
            bool flag2;
            DateTime time;
            DateTime time2;
            GetPasswordWithFormat(username, updateLastLoginActivityDate, out num, out str, out passwordFormat, out salt, out num2, out num3, out flag2, out time, out time2);
            if (num != 0)
            {
                return false;
            }
            if (!flag2 && failIfNotApproved)
            {
                return false;
            }
            var str2 = EncodePassword(password, passwordFormat, salt);
            var objValue = str.Equals(str2);
            if ((objValue && (num2 == 0)) && (num3 == 0))
            {
                return true;
            }
            try
            {
                try
                {
                    connection = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion(connection.Connection);
                    var command = new SqlCommand("dbo.aspnet_Membership_UpdateUserInfo", connection.Connection);
                    var utcNow = DateTime.UtcNow;
                    command.CommandTimeout = CommandTimeout;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    command.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                    command.Parameters.Add(CreateInputParam("@IsPasswordCorrect", SqlDbType.Bit, objValue));
                    command.Parameters.Add(CreateInputParam("@UpdateLastLoginActivityDate", SqlDbType.Bit, updateLastLoginActivityDate));
                    command.Parameters.Add(CreateInputParam("@MaxInvalidPasswordAttempts", SqlDbType.Int, MaxInvalidPasswordAttempts));
                    command.Parameters.Add(CreateInputParam("@PasswordAttemptWindow", SqlDbType.Int, PasswordAttemptWindow));
                    command.Parameters.Add(CreateInputParam("@CurrentTimeUtc", SqlDbType.DateTime, utcNow));
                    command.Parameters.Add(CreateInputParam("@LastLoginDate", SqlDbType.DateTime, objValue ? utcNow : time));
                    command.Parameters.Add(CreateInputParam("@LastActivityDate", SqlDbType.DateTime, objValue ? utcNow : time2));
                    var parameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    command.Parameters.Add(parameter);
                    command.ExecuteNonQuery();
                    num = (parameter.Value != null) ? ((int)parameter.Value) : -1;
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
            return objValue;
        }

        private void CheckSchemaVersion(SqlConnection connection)
        {
            var features = new[] { "Common", "Membership" };
            const string version = "1";
            SecUtility.CheckSchemaVersion(this, connection, features, version, ref _schemaVersionCheck);
        }

        private static SqlParameter CreateInputParam(string paramName, SqlDbType dbType, object objValue)
        {
            var parameter = new SqlParameter(paramName, dbType);
            if (objValue == null)
            {
                parameter.IsNullable = true;
                parameter.Value = DBNull.Value;
                return parameter;
            }
            parameter.Value = objValue;
            return parameter;
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            string str3;
            MembershipUser user;
            if (!SecUtility.ValidateParameter(ref password, true, true, false, 0x80))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            var salt = GenerateSalt();
            var objValue = EncodePassword(password, (int)_passwordFormat, salt);
            if (objValue.Length > 0x80)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            if (passwordAnswer != null)
            {
                passwordAnswer = passwordAnswer.Trim();
            }
            if (!string.IsNullOrEmpty(passwordAnswer))
            {
                if (passwordAnswer.Length > 0x80)
                {
                    status = MembershipCreateStatus.InvalidAnswer;
                    return null;
                }
                str3 = EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), (int)_passwordFormat, salt);
            }
            else
            {
                str3 = passwordAnswer;
            }
            if (!SecUtility.ValidateParameter(ref str3, RequiresQuestionAndAnswer, true, false, 0x80))
            {
                status = MembershipCreateStatus.InvalidAnswer;
                return null;
            }
            if (!SecUtility.ValidateParameter(ref username, true, true, true, 0x100))
            {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }
            if (!SecUtility.ValidateParameter(ref email, RequiresUniqueEmail, RequiresUniqueEmail, false, 0x100))
            {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }
            if (!SecUtility.ValidateParameter(ref passwordQuestion, RequiresQuestionAndAnswer, true, false, 0x100))
            {
                status = MembershipCreateStatus.InvalidQuestion;
                return null;
            }
            if ((providerUserKey != null) && !(providerUserKey is Guid))
            {
                status = MembershipCreateStatus.InvalidProviderUserKey;
                return null;
            }
            if (password.Length < MinRequiredPasswordLength)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            var num = password.Where((t, i) => !char.IsLetterOrDigit(password, i)).Count();
            if (num < MinRequiredNonAlphanumericCharacters)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            if ((PasswordStrengthRegularExpression.Length > 0) && !Regex.IsMatch(password, PasswordStrengthRegularExpression))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            var e = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(e);
            if (e.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            try
            {
                SqlConnectionHolder connection = null;
                try
                {
                    connection = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion(connection.Connection);
                    var time = RoundToSeconds(DateTime.UtcNow);
                    var command = new SqlCommand("dbo.aspnet_Membership_CreateUser", connection.Connection)
                    {
                        CommandTimeout = CommandTimeout,
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    command.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                    command.Parameters.Add(CreateInputParam("@Password", SqlDbType.NVarChar, objValue));
                    command.Parameters.Add(CreateInputParam("@PasswordSalt", SqlDbType.NVarChar, salt));
                    command.Parameters.Add(CreateInputParam("@Email", SqlDbType.NVarChar, email));
                    command.Parameters.Add(CreateInputParam("@PasswordQuestion", SqlDbType.NVarChar, passwordQuestion));
                    command.Parameters.Add(CreateInputParam("@PasswordAnswer", SqlDbType.NVarChar, str3));
                    command.Parameters.Add(CreateInputParam("@IsApproved", SqlDbType.Bit, isApproved));
                    command.Parameters.Add(CreateInputParam("@UniqueEmail", SqlDbType.Int, RequiresUniqueEmail ? 1 : 0));
                    command.Parameters.Add(CreateInputParam("@PasswordFormat", SqlDbType.Int, (int)PasswordFormat));
                    command.Parameters.Add(CreateInputParam("@CurrentTimeUtc", SqlDbType.DateTime, time));
                    var parameter = CreateInputParam("@UserId", SqlDbType.UniqueIdentifier, providerUserKey);
                    parameter.Direction = ParameterDirection.InputOutput;
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    command.Parameters.Add(parameter);
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException exception)
                    {
                        if (((exception.Number != 0xa43) && (exception.Number != 0xa29)) && (exception.Number != 0x9d0))
                        {
                            throw;
                        }
                        status = MembershipCreateStatus.DuplicateUserName;
                        return null;
                    }
                    int num3 = (parameter.Value != null) ? ((int)parameter.Value) : -1;
                    if ((num3 < 0) || (num3 > 11))
                    {
                        num3 = 11;
                    }
                    status = (MembershipCreateStatus)num3;
                    if (num3 != 0)
                    {
                        return null;
                    }
                    providerUserKey = new Guid(command.Parameters["@UserId"].Value.ToString());
                    time = time.ToLocalTime();
                    user = new MembershipUser(Name, username, providerUserKey, email, passwordQuestion, null, isApproved, false, time, time, time, time, new DateTime(0x6da, 1, 1));
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
            return user;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            bool flag;
            SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");
            try
            {
                SqlConnectionHolder connection = null;
                try
                {
                    connection = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion(connection.Connection);
                    var command = new SqlCommand("dbo.aspnet_Users_DeleteUser", connection.Connection)
                    {
                        CommandTimeout = CommandTimeout,
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    command.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                    command.Parameters.Add(deleteAllRelatedData
                                               ? CreateInputParam("@TablesToDeleteFrom", SqlDbType.Int, 15)
                                               : CreateInputParam("@TablesToDeleteFrom", SqlDbType.Int, 1));
                    var parameter = new SqlParameter("@NumTablesDeletedFrom", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(parameter);
                    command.ExecuteNonQuery();
                    var num = (parameter.Value != null) ? ((int)parameter.Value) : -1;
                    flag = num > 0;
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
            return flag;
        }

        private string EncodePassword(string pass, int passwordFormat, string salt)
        {
            if (passwordFormat == 0)
            {
                return pass;
            }
            var bytes = Encoding.Unicode.GetBytes(pass);
            var src = Convert.FromBase64String(salt);
            byte[] inArray;
            if (passwordFormat == 1)
            {
                var hashAlgorithm = GetHashAlgorithm();
                if (hashAlgorithm is KeyedHashAlgorithm)
                {
                    var algorithm2 = (KeyedHashAlgorithm)hashAlgorithm;
                    if (algorithm2.Key.Length == src.Length)
                    {
                        algorithm2.Key = src;
                    }
                    else if (algorithm2.Key.Length < src.Length)
                    {
                        var dst = new byte[algorithm2.Key.Length];
                        Buffer.BlockCopy(src, 0, dst, 0, dst.Length);
                        algorithm2.Key = dst;
                    }
                    else
                    {
                        int num2;
                        var buffer5 = new byte[algorithm2.Key.Length];
                        for (var i = 0; i < buffer5.Length; i += num2)
                        {
                            num2 = Math.Min(src.Length, buffer5.Length - i);
                            Buffer.BlockCopy(src, 0, buffer5, i, num2);
                        }
                        algorithm2.Key = buffer5;
                    }
                    inArray = algorithm2.ComputeHash(bytes);
                }
                else
                {
                    var buffer6 = new byte[src.Length + bytes.Length];
                    Buffer.BlockCopy(src, 0, buffer6, 0, src.Length);
                    Buffer.BlockCopy(bytes, 0, buffer6, src.Length, bytes.Length);
                    inArray = hashAlgorithm.ComputeHash(buffer6);
                }
            }
            else
            {
                var buffer7 = new byte[src.Length + bytes.Length];
                Buffer.BlockCopy(src, 0, buffer7, 0, src.Length);
                Buffer.BlockCopy(bytes, 0, buffer7, src.Length, bytes.Length);
                inArray = EncryptPassword(buffer7, _legacyPasswordCompatibilityMode);
            }
            return Convert.ToBase64String(inArray);
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection users2;
            SecUtility.CheckParameter(ref emailToMatch, false, false, false, 0x100, "emailToMatch");
            if (pageIndex < 0)
            {
                throw new ArgumentException("Bad page index", "pageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException("Bad page size", "pageSize");
            }
            long num = ((pageIndex * pageSize) + pageSize) - 1L;
            if (num > 0x7fffffffL)
            {
                throw new ArgumentException("Page index and page size are bad", "pageIndex");
            }
            try
            {
                SqlConnectionHolder connection = null;
                totalRecords = 0;
                var parameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
                {
                    Direction = ParameterDirection.ReturnValue
                };
                try
                {
                    connection = SqlConnectionHelper.GetConnection();
                    CheckSchemaVersion(connection.Connection);
                    var command = new SqlCommand("dbo.aspnet_Membership_FindUsersByEmail", connection.Connection);
                    var users = new MembershipUserCollection();
                    SqlDataReader reader = null;
                    command.CommandTimeout = CommandTimeout;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    command.Parameters.Add(CreateInputParam("@EmailToMatch", SqlDbType.NVarChar, emailToMatch));
                    command.Parameters.Add(CreateInputParam("@PageIndex", SqlDbType.Int, pageIndex));
                    command.Parameters.Add(CreateInputParam("@PageSize", SqlDbType.Int, pageSize));
                    command.Parameters.Add(parameter);
                    try
                    {
                        reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                        while (reader.Read())
                        {
                            string nullableString = GetNullableString(reader, 0);
                            string email = GetNullableString(reader, 1);
                            string passwordQuestion = GetNullableString(reader, 2);
                            string comment = GetNullableString(reader, 3);
                            bool boolean = reader.GetBoolean(4);
                            DateTime creationDate = reader.GetDateTime(5).ToLocalTime();
                            DateTime lastLoginDate = reader.GetDateTime(6).ToLocalTime();
                            DateTime lastActivityDate = reader.GetDateTime(7).ToLocalTime();
                            DateTime lastPasswordChangedDate = reader.GetDateTime(8).ToLocalTime();
                            Guid providerUserKey = reader.GetGuid(9);
                            bool isLockedOut = reader.GetBoolean(10);
                            DateTime lastLockoutDate = reader.GetDateTime(11).ToLocalTime();
                            users.Add(new MembershipUser(Name, nullableString, providerUserKey, email, passwordQuestion, comment, boolean, isLockedOut, creationDate, lastLoginDate, lastActivityDate, lastPasswordChangedDate, lastLockoutDate));
                        }
                        users2 = users;
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Close();
                        }
                        if ((parameter.Value != null) && (parameter.Value is int))
                        {
                            totalRecords = (int)parameter.Value;
                        }
                    }
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
            return users2;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection users2;
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, 0x100, "usernameToMatch");
            if (pageIndex < 0)
            {
                throw new ArgumentException("Bad page index", "pageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException("Bad page size", "pageSize");
            }
            long num = ((pageIndex * pageSize) + pageSize) - 1L;
            if (num > 0x7fffffffL)
            {
                throw new ArgumentException("Bad page index and size", "pageIndex");
            }
            try
            {
                SqlConnectionHolder connection = null;
                totalRecords = 0;
                var parameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
                {
                    Direction = ParameterDirection.ReturnValue
                };
                try
                {
                    connection = SqlConnectionHelper.GetConnection();
                    CheckSchemaVersion(connection.Connection);
                    var command = new SqlCommand("dbo.aspnet_Membership_FindUsersByName", connection.Connection);
                    var users = new MembershipUserCollection();
                    SqlDataReader reader = null;
                    command.CommandTimeout = CommandTimeout;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    command.Parameters.Add(CreateInputParam("@UserNameToMatch", SqlDbType.NVarChar, usernameToMatch));
                    command.Parameters.Add(CreateInputParam("@PageIndex", SqlDbType.Int, pageIndex));
                    command.Parameters.Add(CreateInputParam("@PageSize", SqlDbType.Int, pageSize));
                    command.Parameters.Add(parameter);
                    try
                    {
                        reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                        while (reader.Read())
                        {
                            string nullableString = GetNullableString(reader, 0);
                            string email = GetNullableString(reader, 1);
                            string passwordQuestion = GetNullableString(reader, 2);
                            string comment = GetNullableString(reader, 3);
                            bool boolean = reader.GetBoolean(4);
                            DateTime creationDate = reader.GetDateTime(5).ToLocalTime();
                            DateTime lastLoginDate = reader.GetDateTime(6).ToLocalTime();
                            DateTime lastActivityDate = reader.GetDateTime(7).ToLocalTime();
                            DateTime lastPasswordChangedDate = reader.GetDateTime(8).ToLocalTime();
                            Guid providerUserKey = reader.GetGuid(9);
                            bool isLockedOut = reader.GetBoolean(10);
                            DateTime lastLockoutDate = reader.GetDateTime(11).ToLocalTime();
                            users.Add(new MembershipUser(Name, nullableString, providerUserKey, email, passwordQuestion, comment, boolean, isLockedOut, creationDate, lastLoginDate, lastActivityDate, lastPasswordChangedDate, lastLockoutDate));
                        }
                        users2 = users;
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Close();
                        }
                        if ((parameter.Value != null) && (parameter.Value is int))
                        {
                            totalRecords = (int)parameter.Value;
                        }
                    }
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
            return users2;
        }

        public virtual string GeneratePassword()
        {
            return Membership.GeneratePassword((MinRequiredPasswordLength < PasswordSize) ? PasswordSize : MinRequiredPasswordLength, MinRequiredNonAlphanumericCharacters);
        }

        private static string GenerateSalt()
        {
            var data = new byte[SaltSize];
            new RNGCryptoServiceProvider().GetBytes(data);
            return Convert.ToBase64String(data);
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            if (pageIndex < 0)
            {
                throw new ArgumentException("Bad page index", "pageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException("Bad page size", "pageSize");
            }
            long num = ((pageIndex * pageSize) + pageSize) - 1L;
            if (num > 0x7fffffffL)
            {
                throw new ArgumentException("Page index and page size are bad", "pageIndex");
            }
            var users = new MembershipUserCollection();
            totalRecords = 0;
            try
            {
                SqlConnectionHolder connection = null;
                try
                {
                    connection = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion(connection.Connection);
                    var command = new SqlCommand("dbo.aspnet_Membership_GetAllUsers", connection.Connection);
                    SqlDataReader reader = null;
                    var parameter = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    command.CommandTimeout = CommandTimeout;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    command.Parameters.Add(CreateInputParam("@PageIndex", SqlDbType.Int, pageIndex));
                    command.Parameters.Add(CreateInputParam("@PageSize", SqlDbType.Int, pageSize));
                    parameter.Direction = ParameterDirection.ReturnValue;
                    command.Parameters.Add(parameter);
                    try
                    {
                        reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                        while (reader.Read())
                        {
                            string nullableString = GetNullableString(reader, 0);
                            string email = GetNullableString(reader, 1);
                            string passwordQuestion = GetNullableString(reader, 2);
                            string comment = GetNullableString(reader, 3);
                            bool boolean = reader.GetBoolean(4);
                            DateTime creationDate = reader.GetDateTime(5).ToLocalTime();
                            DateTime lastLoginDate = reader.GetDateTime(6).ToLocalTime();
                            DateTime lastActivityDate = reader.GetDateTime(7).ToLocalTime();
                            DateTime lastPasswordChangedDate = reader.GetDateTime(8).ToLocalTime();
                            Guid providerUserKey = reader.GetGuid(9);
                            bool isLockedOut = reader.GetBoolean(10);
                            DateTime lastLockoutDate = reader.GetDateTime(11).ToLocalTime();
                            users.Add(new MembershipUser(Name, nullableString, providerUserKey, email, passwordQuestion, comment, boolean, isLockedOut, creationDate, lastLoginDate, lastActivityDate, lastPasswordChangedDate, lastLockoutDate));
                        }
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Close();
                        }
                        if ((parameter.Value != null) && (parameter.Value is int))
                        {
                            totalRecords = (int)parameter.Value;
                        }
                    }
                    return users;
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private string GetEncodedPasswordAnswer(string username, string passwordAnswer)
        {
            int num;
            int num2;
            int num3;
            int num4;
            string str;
            string str2;
            bool flag;
            DateTime time;
            DateTime time2;
            if (passwordAnswer != null)
            {
                passwordAnswer = passwordAnswer.Trim();
            }
            if (string.IsNullOrEmpty(passwordAnswer))
            {
                return passwordAnswer;
            }
            GetPasswordWithFormat(username, false, out num, out str, out num2, out str2, out num3, out num4, out flag, out time, out time2);
            if (num != 0)
            {
                throw new ProviderException(GetExceptionText(num));
            }
            return EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), num2, str2);
        }

        private static string GetExceptionText(int status)
        {
            switch (status)
            {
                case 0: return string.Empty;
                case 1: return "User has not been found";
                case 2: return "Wrong password";
                case 3: return "Wrong answer";
                case 4: return "Invalid password";
                case 5: return "Invalid question";
                case 6: return "Invalid answer";
                case 7: return "Invalid email";
                case 0x63: return "Account is locked out";
                default: return "Naviam Membership Provider error";
            }
        }

        private HashAlgorithm GetHashAlgorithm()
        {
            if (_sHashAlgorithm != null)
            {
                return HashAlgorithm.Create(_sHashAlgorithm);
            }
            var hashAlgorithmType = Membership.HashAlgorithmType;
            if ((_legacyPasswordCompatibilityMode == MembershipPasswordCompatibilityMode.Framework20) && (hashAlgorithmType != "MD5")) // && !Membership.IsHashAlgorithmFromMembershipConfig
            {
                hashAlgorithmType = "SHA1";
            }
            var algorithm = HashAlgorithm.Create(hashAlgorithmType);
            _sHashAlgorithm = hashAlgorithmType;
            return algorithm;
        }

        private static string GetNullableString(IDataRecord reader, int col)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            return !reader.IsDBNull(col) ? reader.GetString(col) : null;
        }

        public override int GetNumberOfUsersOnline()
        {
            int num2;
            try
            {
                SqlConnectionHolder connection = null;
                try
                {
                    connection = SqlConnectionHelper.GetConnection();
                    CheckSchemaVersion(connection.Connection);
                    var command = new SqlCommand("dbo.aspnet_Membership_GetNumberOfUsersOnline", connection.Connection);
                    var parameter = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    command.CommandTimeout = CommandTimeout;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    command.Parameters.Add(CreateInputParam("@MinutesSinceLastInActive", SqlDbType.Int, Membership.UserIsOnlineTimeWindow));
                    command.Parameters.Add(CreateInputParam("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
                    parameter.Direction = ParameterDirection.ReturnValue;
                    command.Parameters.Add(parameter);
                    command.ExecuteNonQuery();
                    var num = (parameter.Value != null) ? ((int)parameter.Value) : -1;
                    num2 = num;
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
            return num2;
        }

        public override string GetPassword(string username, string passwordAnswer)
        {
            if (!EnablePasswordRetrieval)
            {
                throw new NotSupportedException("Password retrieval is not supported");
            }
            SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");
            var encodedPasswordAnswer = GetEncodedPasswordAnswer(username, passwordAnswer);
            SecUtility.CheckParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 0x80, "passwordAnswer");
            int passwordFormat;
            int status;
            var pass = GetPasswordFromDb(username, encodedPasswordAnswer, RequiresQuestionAndAnswer, out passwordFormat, out status);
            if (pass != null)
            {
                return UnEncodePassword(pass, passwordFormat);
            }
            var exceptionText = GetExceptionText(status);
            if (IsStatusDueToBadPassword(status))
            {
                throw new MembershipPasswordException(exceptionText);
            }
            throw new ProviderException(exceptionText);
        }

        private string GetPasswordFromDb(string username, string passwordAnswer, bool requiresQuestionAndAnswer, out int passwordFormat, out int status)
        {
            string str2;
            try
            {
                SqlConnectionHolder connection = null;
                SqlDataReader reader = null;
                SqlParameter parameter = null;
                try
                {
                    connection = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion(connection.Connection);
                    var command = new SqlCommand("dbo.aspnet_Membership_GetPassword", connection.Connection)
                    {
                        CommandTimeout = CommandTimeout,
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    command.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                    command.Parameters.Add(CreateInputParam("@MaxInvalidPasswordAttempts", SqlDbType.Int, MaxInvalidPasswordAttempts));
                    command.Parameters.Add(CreateInputParam("@PasswordAttemptWindow", SqlDbType.Int, PasswordAttemptWindow));
                    command.Parameters.Add(CreateInputParam("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
                    if (requiresQuestionAndAnswer)
                    {
                        command.Parameters.Add(CreateInputParam("@PasswordAnswer", SqlDbType.NVarChar, passwordAnswer));
                    }
                    parameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    command.Parameters.Add(parameter);
                    reader = command.ExecuteReader(CommandBehavior.SingleRow);
                    string str;
                    status = -1;
                    if (reader.Read())
                    {
                        str = reader.GetString(0);
                        passwordFormat = reader.GetInt32(1);
                    }
                    else
                    {
                        str = null;
                        passwordFormat = 0;
                    }
                    str2 = str;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        status = (parameter.Value != null) ? ((int)parameter.Value) : -1;
                    }
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
            return str2;
        }

        private void GetPasswordWithFormat(string username, bool updateLastLoginActivityDate, out int status, out string password, out int passwordFormat, out string passwordSalt, out int failedPasswordAttemptCount, out int failedPasswordAnswerAttemptCount, out bool isApproved, out DateTime lastLoginDate, out DateTime lastActivityDate)
        {
            try
            {
                SqlConnectionHolder connection = null;
                SqlDataReader reader = null;
                SqlParameter parameter = null;
                try
                {
                    connection = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion(connection.Connection);
                    var command = new SqlCommand("dbo.aspnet_Membership_GetPasswordWithFormat", connection.Connection)
                    {
                        CommandTimeout = CommandTimeout,
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    command.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                    command.Parameters.Add(CreateInputParam("@UpdateLastLoginActivityDate", SqlDbType.Bit, updateLastLoginActivityDate));
                    command.Parameters.Add(CreateInputParam("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
                    parameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    command.Parameters.Add(parameter);
                    reader = command.ExecuteReader(CommandBehavior.SingleRow);
                    status = -1;
                    if (reader.Read())
                    {
                        password = reader.GetString(0);
                        passwordFormat = reader.GetInt32(1);
                        passwordSalt = reader.GetString(2);
                        failedPasswordAttemptCount = reader.GetInt32(3);
                        failedPasswordAnswerAttemptCount = reader.GetInt32(4);
                        isApproved = reader.GetBoolean(5);
                        lastLoginDate = reader.GetDateTime(6);
                        lastActivityDate = reader.GetDateTime(7);
                    }
                    else
                    {
                        password = null;
                        passwordFormat = 0;
                        passwordSalt = null;
                        failedPasswordAttemptCount = 0;
                        failedPasswordAnswerAttemptCount = 0;
                        isApproved = false;
                        lastLoginDate = DateTime.UtcNow;
                        lastActivityDate = DateTime.UtcNow;
                    }
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        status = (parameter.Value != null) ? ((int)parameter.Value) : -1;
                    }
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            if (providerUserKey == null)
            {
                throw new ArgumentNullException("providerUserKey");
            }
            if (!(providerUserKey is Guid))
            {
                throw new ArgumentException("Invalid provider user key", "providerUserKey");
            }
            SqlDataReader reader = null;
            try
            {
                SqlConnectionHolder connection = null;
                try
                {
                    connection = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion(connection.Connection);
                    var command = new SqlCommand("dbo.aspnet_Membership_GetUserByUserId", connection.Connection)
                    {
                        CommandTimeout = CommandTimeout,
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add(CreateInputParam("@UserId", SqlDbType.UniqueIdentifier, providerUserKey));
                    command.Parameters.Add(CreateInputParam("@UpdateLastActivity", SqlDbType.Bit, userIsOnline));
                    command.Parameters.Add(CreateInputParam("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
                    var parameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    command.Parameters.Add(parameter);
                    reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        string nullableString = GetNullableString(reader, 0);
                        string passwordQuestion = GetNullableString(reader, 1);
                        string comment = GetNullableString(reader, 2);
                        bool boolean = reader.GetBoolean(3);
                        DateTime creationDate = reader.GetDateTime(4).ToLocalTime();
                        DateTime lastLoginDate = reader.GetDateTime(5).ToLocalTime();
                        DateTime lastActivityDate = reader.GetDateTime(6).ToLocalTime();
                        DateTime lastPasswordChangedDate = reader.GetDateTime(7).ToLocalTime();
                        string name = GetNullableString(reader, 8);
                        bool isLockedOut = reader.GetBoolean(9);
                        return new MembershipUser(Name, name, providerUserKey, nullableString, passwordQuestion, comment, boolean, isLockedOut, creationDate, lastLoginDate, lastActivityDate, lastPasswordChangedDate, reader.GetDateTime(10).ToLocalTime());
                    }
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
            return null;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            SecUtility.CheckParameter(ref username, true, false, true, 0x100, "username");
            SqlDataReader reader = null;
            try
            {
                SqlConnectionHolder connection = null;
                try
                {
                    connection = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion(connection.Connection);
                    var command = new SqlCommand("dbo.aspnet_Membership_GetUserByName", connection.Connection)
                    {
                        CommandTimeout = CommandTimeout,
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    command.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                    command.Parameters.Add(CreateInputParam("@UpdateLastActivity", SqlDbType.Bit, userIsOnline));
                    command.Parameters.Add(CreateInputParam("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
                    var parameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    command.Parameters.Add(parameter);
                    reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        string nullableString = GetNullableString(reader, 0);
                        string passwordQuestion = GetNullableString(reader, 1);
                        string comment = GetNullableString(reader, 2);
                        bool boolean = reader.GetBoolean(3);
                        DateTime creationDate = reader.GetDateTime(4).ToLocalTime();
                        DateTime lastLoginDate = reader.GetDateTime(5).ToLocalTime();
                        DateTime lastActivityDate = reader.GetDateTime(6).ToLocalTime();
                        DateTime lastPasswordChangedDate = reader.GetDateTime(7).ToLocalTime();
                        Guid providerUserKey = reader.GetGuid(8);
                        bool isLockedOut = reader.GetBoolean(9);
                        return new MembershipUser(Name, username, providerUserKey, nullableString, passwordQuestion, comment, boolean, isLockedOut, creationDate, lastLoginDate, lastActivityDate, lastPasswordChangedDate, reader.GetDateTime(10).ToLocalTime());
                    }
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
            return null;
        }

        public override string GetUserNameByEmail(string email)
        {
            string str2;
            SecUtility.CheckParameter(ref email, false, false, false, 0x100, "email");
            try
            {
                SqlConnectionHolder connection = null;
                try
                {
                    connection = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion(connection.Connection);
                    var command = new SqlCommand("dbo.aspnet_Membership_GetUserByEmail", connection.Connection);
                    string nullableString = null;
                    SqlDataReader reader = null;
                    command.CommandTimeout = CommandTimeout;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    command.Parameters.Add(CreateInputParam("@Email", SqlDbType.NVarChar, email));
                    var parameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    command.Parameters.Add(parameter);
                    try
                    {
                        reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                        if (reader.Read())
                        {
                            nullableString = GetNullableString(reader, 0);
                            if (RequiresUniqueEmail && reader.Read())
                            {
                                throw new ProviderException("Membership_more_than_one_user_with_email");
                            }
                        }
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Close();
                        }
                    }
                    str2 = nullableString;
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
            return str2;
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            //HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, "Feature_not_supported_at_this_level");
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (string.IsNullOrEmpty(name))
            {
                name = "NaviamMembershipProvider";
            }
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Naviam Membership Provider");
            }
            base.Initialize(name, config);
            _schemaVersionCheck = 0;
            _enablePasswordRetrieval = SecUtility.GetBooleanValue(config, "enablePasswordRetrieval", false);
            _enablePasswordReset = SecUtility.GetBooleanValue(config, "enablePasswordReset", true);
            _requiresQuestionAndAnswer = SecUtility.GetBooleanValue(config, "requiresQuestionAndAnswer", true);
            _requiresUniqueEmail = SecUtility.GetBooleanValue(config, "requiresUniqueEmail", true);
            _maxInvalidPasswordAttempts = SecUtility.GetIntValue(config, "maxInvalidPasswordAttempts", 5, false, 0);
            _passwordAttemptWindow = SecUtility.GetIntValue(config, "passwordAttemptWindow", 10, false, 0);
            _minRequiredPasswordLength = SecUtility.GetIntValue(config, "minRequiredPasswordLength", 7, false, 0x80);
            _minRequiredNonalphanumericCharacters = SecUtility.GetIntValue(config, "minRequiredNonalphanumericCharacters", 1, true, 0x80);
            _passwordStrengthRegularExpression = config["passwordStrengthRegularExpression"];
            if (_passwordStrengthRegularExpression != null)
            {
                _passwordStrengthRegularExpression = _passwordStrengthRegularExpression.Trim();
                if (_passwordStrengthRegularExpression.Length == 0)
                {
                    goto Label_016C;
                }
                try
                {
                    new Regex(_passwordStrengthRegularExpression);
                    goto Label_016C;
                }
                catch (ArgumentException exception)
                {
                    throw new ProviderException(exception.Message, exception);
                }
            }
            _passwordStrengthRegularExpression = string.Empty;
        Label_016C:
            if (_minRequiredNonalphanumericCharacters > _minRequiredPasswordLength)
            {
                throw new HttpException("MinRequiredNonalphanumericCharacters can not be more than MinRequiredPasswordLength");
            }
            CommandTimeout = SecUtility.GetIntValue(config, "commandTimeout", 30, true, 0);
            _appName = config["applicationName"];
            if (string.IsNullOrEmpty(_appName))
            {
                _appName = SecUtility.GetDefaultAppName();
            }
            if (_appName.Length > 0x100)
            {
                throw new ProviderException("Application name too long");
            }
            var str = config["passwordFormat"] ?? "Hashed";
            var str4 = str;
            if (str4 != "Clear")
            {
                if (str4 == "Encrypted")
                {
                    _passwordFormat = MembershipPasswordFormat.Encrypted;
                    goto Label_025C;
                }
                if (str4 == "Hashed")
                {
                    _passwordFormat = MembershipPasswordFormat.Hashed;
                    goto Label_025C;
                }
            }
            else
            {
                _passwordFormat = MembershipPasswordFormat.Clear;
                goto Label_025C;
            }
            throw new ProviderException("Bad password format");
        Label_025C:
            if ((PasswordFormat == MembershipPasswordFormat.Hashed) && EnablePasswordRetrieval)
            {
                throw new ProviderException("Cannot retrieve hashed password");
            }
            _sqlConnectionString = SecUtility.GetConnectionString(config);
            var str2 = config["passwordCompatMode"];
            if (!string.IsNullOrEmpty(str2))
            {
                _legacyPasswordCompatibilityMode = (MembershipPasswordCompatibilityMode)Enum.Parse(typeof(MembershipPasswordCompatibilityMode), str2);
            }
            config.Remove("connectionStringName");
            config.Remove("connectionString");
            config.Remove("enablePasswordRetrieval");
            config.Remove("enablePasswordReset");
            config.Remove("requiresQuestionAndAnswer");
            config.Remove("applicationName");
            config.Remove("requiresUniqueEmail");
            config.Remove("maxInvalidPasswordAttempts");
            config.Remove("passwordAttemptWindow");
            config.Remove("commandTimeout");
            config.Remove("passwordFormat");
            config.Remove("name");
            config.Remove("minRequiredPasswordLength");
            config.Remove("minRequiredNonalphanumericCharacters");
            config.Remove("passwordStrengthRegularExpression");
            config.Remove("passwordCompatMode");
            if (config.Count > 0)
            {
                var key = config.GetKey(0);
                if (!string.IsNullOrEmpty(key))
                {
                    throw new ProviderException(String.Format("Unrecognized attribute: {0}", key));
                }
            }
        }

        private static bool IsStatusDueToBadPassword(int status)
        {
            return (((status >= 2) && (status <= 6)) || (status == 0x63));
        }

        public override string ResetPassword(string username, string passwordAnswer)
        {
            string str;
            int num;
            string str2;
            int num2;
            int num3;
            int num4;
            bool flag;
            DateTime time;
            DateTime time2;
            if (!EnablePasswordReset)
            {
                throw new NotSupportedException("Password reset support is not configured");
            }
            SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");
            GetPasswordWithFormat(username, false, out num2, out str2, out num, out str, out num3, out num4, out flag, out time, out time2);
            if (num2 == 0)
            {
                string str6;
                if (passwordAnswer != null)
                {
                    passwordAnswer = passwordAnswer.Trim();
                }
                var str3 = !string.IsNullOrEmpty(passwordAnswer) ? EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), num, str) : passwordAnswer;
                SecUtility.CheckParameter(ref str3, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 0x80, "passwordAnswer");
                var password = GeneratePassword();
                var e = new ValidatePasswordEventArgs(username, password, false);
                OnValidatingPassword(e);
                if (e.Cancel)
                {
                    if (e.FailureInformation != null)
                    {
                        throw e.FailureInformation;
                    }
                    throw new ProviderException("Password validation failure");
                }
                try
                {
                    SqlConnectionHolder connection = null;
                    try
                    {
                        connection = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                        CheckSchemaVersion(connection.Connection);
                        var command = new SqlCommand("dbo.aspnet_Membership_ResetPassword", connection.Connection)
                        {
                            CommandTimeout = CommandTimeout,
                            CommandType = CommandType.StoredProcedure
                        };
                        command.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                        command.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                        command.Parameters.Add(CreateInputParam("@NewPassword", SqlDbType.NVarChar, EncodePassword(password, num, str)));
                        command.Parameters.Add(CreateInputParam("@MaxInvalidPasswordAttempts", SqlDbType.Int, MaxInvalidPasswordAttempts));
                        command.Parameters.Add(CreateInputParam("@PasswordAttemptWindow", SqlDbType.Int, PasswordAttemptWindow));
                        command.Parameters.Add(CreateInputParam("@PasswordSalt", SqlDbType.NVarChar, str));
                        command.Parameters.Add(CreateInputParam("@PasswordFormat", SqlDbType.Int, num));
                        command.Parameters.Add(CreateInputParam("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
                        if (RequiresQuestionAndAnswer)
                        {
                            command.Parameters.Add(CreateInputParam("@PasswordAnswer", SqlDbType.NVarChar, str3));
                        }
                        var parameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.ReturnValue
                        };
                        command.Parameters.Add(parameter);
                        command.ExecuteNonQuery();
                        num2 = (parameter.Value != null) ? ((int)parameter.Value) : -1;
                        if (num2 != 0)
                        {
                            string exceptionText = GetExceptionText(num2);
                            if (IsStatusDueToBadPassword(num2))
                            {
                                throw new MembershipPasswordException(exceptionText);
                            }
                            throw new ProviderException(exceptionText);
                        }
                        str6 = password;
                    }
                    finally
                    {
                        if (connection != null)
                        {
                            connection.Close();
                        }
                    }
                }
                catch
                {
                    throw;
                }
                return str6;
            }
            if (IsStatusDueToBadPassword(num2))
            {
                throw new MembershipPasswordException(GetExceptionText(num2));
            }
            throw new ProviderException(GetExceptionText(num2));
        }

        private static DateTime RoundToSeconds(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
        }

        private string UnEncodePassword(string pass, int passwordFormat)
        {
            switch (passwordFormat)
            {
                case 0:
                    return pass;
                case 1:
                    throw new ProviderException("Cannot decode hashed password");
            }
            var encodedPassword = Convert.FromBase64String(pass);
            var bytes = DecryptPassword(encodedPassword);
            return bytes == null ? null : Encoding.Unicode.GetString(bytes, SaltSize, bytes.Length - SaltSize);
        }

        public override bool UnlockUser(string username)
        {
            SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");
            try
            {
                SqlConnectionHolder connection = null;
                try
                {
                    connection = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion(connection.Connection);
                    var command = new SqlCommand("dbo.aspnet_Membership_UnlockUser", connection.Connection)
                    {
                        CommandTimeout = CommandTimeout,
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    command.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                    var parameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    command.Parameters.Add(parameter);
                    command.ExecuteNonQuery();
                    if (((parameter.Value != null) ? ((int)parameter.Value) : -1) == 0)
                    {
                        return true;
                    }
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
            return false;
        }

        public override void UpdateUser(MembershipUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            //SecUtility.CheckParameter(ref user.UserName, true, true, true, 0x100, "UserName");
            var email = user.Email;
            SecUtility.CheckParameter(ref email, RequiresUniqueEmail, RequiresUniqueEmail, false, 0x100, "Email");
            user.Email = email;
            try
            {
                SqlConnectionHolder connection = null;
                try
                {
                    connection = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion(connection.Connection);
                    var command = new SqlCommand("dbo.aspnet_Membership_UpdateUser", connection.Connection)
                    {
                        CommandTimeout = CommandTimeout,
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    command.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, user.UserName));
                    command.Parameters.Add(CreateInputParam("@Email", SqlDbType.NVarChar, user.Email));
                    command.Parameters.Add(CreateInputParam("@Comment", SqlDbType.NText, user.Comment));
                    command.Parameters.Add(CreateInputParam("@IsApproved", SqlDbType.Bit, user.IsApproved ? 1 : 0));
                    command.Parameters.Add(CreateInputParam("@LastLoginDate", SqlDbType.DateTime, user.LastLoginDate.ToUniversalTime()));
                    command.Parameters.Add(CreateInputParam("@LastActivityDate", SqlDbType.DateTime, user.LastActivityDate.ToUniversalTime()));
                    command.Parameters.Add(CreateInputParam("@UniqueEmail", SqlDbType.Int, RequiresUniqueEmail ? 1 : 0));
                    command.Parameters.Add(CreateInputParam("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
                    var parameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    command.Parameters.Add(parameter);
                    command.ExecuteNonQuery();
                    var status = (parameter.Value != null) ? ((int)parameter.Value) : -1;
                    if (status != 0)
                    {
                        throw new ProviderException(GetExceptionText(status));
                    }
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public override bool ValidateUser(string username, string password)
        {
            if ((SecUtility.ValidateParameter(ref username, true, true, true, 0x100) && 
                SecUtility.ValidateParameter(ref password, true, true, false, 0x80)) && 
                CheckPassword(username, password, true, true))
            {
                //PerfCounters.IncrementCounter(AppPerfCounter.MEMBER_SUCCESS);
                //WebBaseEvent.RaiseSystemEvent(null, 0xfa2, username);
                return true;
            }
            //PerfCounters.IncrementCounter(AppPerfCounter.MEMBER_FAIL);
            //WebBaseEvent.RaiseSystemEvent(null, 0xfa6, username);
            return false;
        }

        #region PROPERTIES
        public override string ApplicationName
        {
            get
            {
                return _appName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(value, "Application property is null");
                }
                if (value.Length > 0x100)
                {
                    throw new ProviderException("Application name is too long");
                }
                _appName = value;
            }
        }

        private int CommandTimeout { get; set; }

        public override bool EnablePasswordReset
        {
            get
            {
                return _enablePasswordReset;
            }
        }

        public override bool EnablePasswordRetrieval
        {
            get
            {
                return _enablePasswordRetrieval;
            }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get
            {
                return _maxInvalidPasswordAttempts;
            }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                return _minRequiredNonalphanumericCharacters;
            }
        }

        public override int MinRequiredPasswordLength
        {
            get
            {
                return _minRequiredPasswordLength;
            }
        }

        public override int PasswordAttemptWindow
        {
            get
            {
                return _passwordAttemptWindow;
            }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get
            {
                return _passwordFormat;
            }
        }

        public override string PasswordStrengthRegularExpression
        {
            get
            {
                return _passwordStrengthRegularExpression;
            }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get
            {
                return _requiresQuestionAndAnswer;
            }
        }

        public override bool RequiresUniqueEmail
        {
            get
            {
                return _requiresUniqueEmail;
            }
        } 
        #endregion
    }
}