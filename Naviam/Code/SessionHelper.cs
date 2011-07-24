using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Naviam.Data;

namespace Naviam.Code
{
    public class SessionHelper
    {
        #region PROPERTY::UserProfile
        /// <summary>
        /// Get UserProfile from the session state
        /// </summary>
        public static UserProfile UserProfile
        {
            get
            {
                return HttpContext.Current.Session["userprofile"] as UserProfile;
            }
            set
            {
                HttpContext.Current.Session["userprofile"] = value;
            }
        }
        #endregion
    }
}