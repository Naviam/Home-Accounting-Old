using System;
using System.Web;
using System.Web.Mvc;
using Naviam.Data;
using Naviam.WebUI.Helpers;

namespace Naviam.WebUI.Controllers
{
    // fixes an issue with browser's back button after logout
    //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class BaseController : Controller
    {
        /// <summary>
        /// Get current user from session or redis
        /// </summary>
        protected UserProfile CurrentUser { get; set; }

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            CurrentUser = SessionHelper.UserProfile;
            ViewBag.CurrentUser = CurrentUser;
            if (CurrentUser == null)
                filterContext.Result = new HttpUnauthorizedResult();
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
            if (filterContext == null) return;
            Exception ex = filterContext.Exception;
            // If this is not an HTTP 500 (for example, if somebody throws an HTTP 404 from an action method), ignore it.
            if (new HttpException(null, ex).GetHttpCode() != (int)System.Net.HttpStatusCode.InternalServerError)
            {
                return;
            }
            var request = filterContext.HttpContext.Request;
            if (request.IsAjaxRequest())
            {
                filterContext.Result = Json(new { header = "Error", Text = ex.Message, stackTrace = ex.StackTrace }, JsonRequestBehavior.AllowGet);
                filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                filterContext.ExceptionHandled = true;
            }
            else
            {
                //if (request.UrlReferrer != null)
                //    filterContext.HttpContext.Response.Redirect(request.UrlReferrer.PathAndQuery);
                //TempData["ErrorContextText"] = ex.Message.Replace("\n", "").Replace("\r", "");
                //filterContext.ExceptionHandled = true;
                //filterContext.HttpContext.Response.StatusCode = 200;
            }
        }

    }


}
