﻿using System.Web;
using System.Web.Security;
using Naviam.Entities.User;

namespace Naviam.WebUI.Helpers
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
                var res = CacheWrapper.Get<UserProfile>(cId);
                if (res != null && FormsAuthentication.SlidingExpiration) //sliding expiration in redis
                    CacheWrapper.ProlongKey(cId);
                return res; //HttpContext.Current.Session["userprofile"] as UserProfile;
            }
            set
            {
                string cId = ContextId;
                if (cId == null)
                    return;
                CacheWrapper.Set<UserProfile>(cId, value, true, null); //HttpContext.Current.Session["userprofile"] = value;
            }
        }

        public static void SetNewUserProfile(string cId, UserProfile user)
        {
            CacheWrapper.Set<UserProfile>(cId, user, true, null);
        }
        #endregion
    }
}