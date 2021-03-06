﻿using System;
using System.Collections.Generic;
using System.Data;
using Naviam.Data;
using System.Web;
using System.Xml.Serialization;
using System.Globalization;
using System.Data.SqlClient;

namespace Naviam.Data
{
    /// <summary>
    /// User Profile object
    /// </summary>
    [Serializable]
    public class UserProfile : DbEntity
    {
        private CultureInfo _ci;
        private string _language_short_name;

        public UserProfile()  {}

        public UserProfile(string email, string password, string approveCode)
        {
            Name = email;
            Password = password;
            ApproveCode = approveCode;
        }

        public UserProfile(IDataRecord record)
        {
            Id = record["id"] as int?;
            Name = record["email"] as string;
            Password = record["password"] as string;
            FirstName = record["first_name"] as string;
            LastName = record["last_name"] as string;
            Comment = record["comment"] as string;
            PasswordQuestion = record["password_question"] as string;
            PasswordAnswer = record["password_answer"] as string;
            LanguageId = record["id_language"] as int?;
            LanguageNameShort = record["language_name_short"] as string;
            IsApproved = (record["is_approved"] as bool?).HasTrue();
            CreationDate = record["creation_date"] as DateTime?;
            DefaultCompany = record["id_company"] as int?;
            ApproveCode = record["approve_code"] as string;
        }

        public string Name { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? LanguageId { get; set; }
        public string LanguageNameShort
        { 
            get 
            { 
                return _language_short_name; 
            }
            set 
            {
                _language_short_name = value;
                if(string.IsNullOrEmpty(_language_short_name))
                    _ci = new CultureInfo("en");
                else
                    _ci = new CultureInfo(LanguageNameShort);
            }
        }
        public int? DefaultCompany { get; set; }
        [XmlIgnore]
        public int? CurrentCompany { 
            get 
            {
                int? res = null;
                var context = HttpContext.Current;
                if (context != null)
                {
                    res = context.Request.QueryString["cid"] != null ? (int?)Convert.ToInt32(context.Request.QueryString["cid"]) : null;
                    var frmReq = context.Request.Form["pageContext[companyId]"];
                    if (res == null)
                        res = !String.IsNullOrEmpty(frmReq) ? (int?)Convert.ToInt32(frmReq) : null;
                }
                bool found = false;
                if (res == null)
                    res = DefaultCompany;
                foreach (var company in Companies)
                {
                    if (company.Id == res)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    throw new Exception("Access denied.");
                return res;
            } 
        }
        public IEnumerable<Company> Companies { get; set; }
        public string Comment { get; set; }
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? CreationDate { get; set; }
        public string ApproveCode { get; set; }
        public Exception LastException { get; set; }
    }

    public static partial class SqlCommandExtensions
    {
        /// <summary>
        /// Appends UserProfile-specific parameters to the specificied SqlCommand. 
        /// </summary>
        /// <param name="command">SqlCommand to be executed.</param>
        /// <param name="alert">Instance of UserProfile class</param>
        /// <param name="action">Database action type (select, insert, update, delete).</param>
        public static void AddEntityParameters(this SqlCommand command, UserProfile userProfile, DbActionType action)
        {
            command.AddCommonParameters(userProfile.Id, action);
            command.Parameters.Add("@email", SqlDbType.NVarChar).Value = userProfile.Name.ToDbValue(); 
            command.Parameters.Add("@password", SqlDbType.NVarChar).Value = userProfile.Password.ToDbValue();
            command.Parameters.Add("@password_question", SqlDbType.NVarChar).Value = userProfile.PasswordQuestion.ToDbValue(); 
            command.Parameters.Add("@password_answer", SqlDbType.NVarChar).Value = userProfile.PasswordAnswer.ToDbValue(); 
            command.Parameters.Add("@first_name", SqlDbType.NVarChar).Value = userProfile.FirstName.ToDbValue(); 
            command.Parameters.Add("@last_name", SqlDbType.NVarChar).Value = userProfile.LastName.ToDbValue();
            command.Parameters.Add("@comment", SqlDbType.NVarChar).Value = userProfile.Comment.ToDbValue();
            command.Parameters.Add("@is_approved", SqlDbType.Bit).Value = userProfile.IsApproved;
            command.Parameters.Add("@current_time_utc", SqlDbType.DateTime).Value = DateTime.UtcNow;
        }
    }
}
