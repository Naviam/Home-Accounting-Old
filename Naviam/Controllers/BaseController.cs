﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Naviam.Data;
using Naviam.Code;

namespace Naviam.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            UserProfile user = SessionHelper.UserProfile;
            if (user == null)
                filterContext.Result = new HttpUnauthorizedResult();
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
            if (filterContext == null) return;
            if (!filterContext.HttpContext.Request.IsAjaxRequest()) return;
            Exception ex = filterContext.Exception;
            // If this is not an HTTP 500 (for example, if somebody throws an HTTP 404 from an action method),
            // ignore it.
            if (new HttpException(null, ex).GetHttpCode() != (int)System.Net.HttpStatusCode.InternalServerError)
            {
                return;
            }
            filterContext.Result = Json(new { header = "Error", Text = ex.Message, stackTrace = ex.StackTrace }, JsonRequestBehavior.AllowGet);
            filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
            filterContext.ExceptionHandled = true;
        }

    }


}
