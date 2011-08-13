using System;
using System.Collections.Generic;
using System.Web.Security;
using Naviam.Data;

namespace Naviam.Domain.Entities
{
    public class NaviamMembershipUser : MembershipUser
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? DefaultCompany { get; set; }
        public IEnumerable<Company> Companies { get; set; }

        public NaviamMembershipUser(string providername,
                                  string username,
                                  object providerUserKey,
                                  string email,
                                  string passwordQuestion,
                                  string comment,
                                  bool isApproved,
                                  bool isLockedOut,
                                  DateTime creationDate,
                                  DateTime lastLoginDate,
                                  DateTime lastActivityDate,
                                  DateTime lastPasswordChangedDate,
                                  DateTime lastLockedOutDate,
                                  string firstName,
                                  string lastName,
                                  int? defaultCompany) : base(providername,
                                       username,
                                       providerUserKey,
                                       email,
                                       passwordQuestion,
                                       comment,
                                       isApproved,
                                       isLockedOut,
                                       creationDate,
                                       lastLoginDate,
                                       lastActivityDate,
                                       lastPasswordChangedDate,
                                       lastLockedOutDate)
        {
            FirstName = firstName;
            LastName = lastName;
            DefaultCompany = defaultCompany;
        }
    }
}