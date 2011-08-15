using System.Web;
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
                var context = HttpContext.Current;
                var identity = ((context != null) && (context.User != null)) ? (context.User.Identity as FormsIdentity) : null;
                return ((identity != null) && identity.IsAuthenticated) ? identity.Ticket : null;
            }
        }
        public static string ContextId
        {
            get
            {
                var ticket = AuthenticationTicket;
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
                var cache = new CacheWrapper();
                var cId = ContextId;
                if (cId == null)
                    return null;
                var res = cache.Get<UserProfile>(cId);
                if (res != null && FormsAuthentication.SlidingExpiration) //sliding expiration in redis
                    cache.ProlongKey(cId);
                return res; //HttpContext.Current.Session["userprofile"] as UserProfile;
            }
            set
            {
                var cache = new CacheWrapper();
                var cId = ContextId;
                if (cId == null)
                    return;
                cache.Set(cId, value, true, null); //HttpContext.Current.Session["userprofile"] = value;
            }
        }

        //public static void SetNewUserProfile(string cId, UserProfile user)
        //{
        //    CacheWrapper.GetInstance().Set(cId, user, true, null);
        //}
        #endregion
    }
}