using System;
using System.Collections.Generic;
using System.Data;
using Naviam.Data;
using System.Web;
using System.Xml.Serialization;
using System.Globalization;

namespace Naviam.Entities.User
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
                    if (res == null)
                        res = DefaultCompany;
                }
                bool found = false;
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
        //public CultureInfo Culture
        //{
        //    get
        //    {
        //        return _ci;
        //    }
        
        //}
    }
}
