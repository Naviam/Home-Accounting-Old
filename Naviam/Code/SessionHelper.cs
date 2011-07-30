using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Naviam.Data;
using System.Web.Security;

namespace Naviam.Code
{
    public class SessionHelper
    {
        public static FormsAuthenticationTicket AuthenticationTicket
        {
            get
            {
                HttpContext context = HttpContext.Current;
                FormsIdentity identity = ((context != null) && (context.User != null)) ? (context.User.Identity as FormsIdentity) : null;
                return ((identity != null) && identity.IsAuthenticated) ? identity.Ticket : null;
            }
        }
        public static string ContextId
        {
            get
            {
                FormsAuthenticationTicket ticket = AuthenticationTicket;
                return (null != ticket) ? ticket.UserData : null;
            }
        }

        #region PROPERTY::UserProfile
        /// <summary>
        /// Get UserProfile from the session state
        /// </summary>
        public static UserProfile UserProfile
        {
            get
            {
                string cId = ContextId;
                if (cId == null)
                    return null;
                UserProfile res = CacheWrapper.Get<UserProfile>(cId);
                if (res != null && FormsAuthentication.SlidingExpiration) //sliding expiration in redis
                    CacheWrapper.ProlongKey(cId);
                return res; //HttpContext.Current.Session["userprofile"] as UserProfile;
            }
            set
            {
                string cId = ContextId;
                if (cId == null)
                    return;
                CacheWrapper.Set<UserProfile>(cId, value, null, true); //HttpContext.Current.Session["userprofile"] = value;
            }
        }

        public static void SetNewUserProfile(string cId, UserProfile user)
        {
            CacheWrapper.Set<UserProfile>(cId, user, null, true);
        }
        #endregion
    }
}