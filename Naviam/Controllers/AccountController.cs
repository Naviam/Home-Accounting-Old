using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Naviam.Models;

using Naviam.DAL;
using Naviam.Data;
using Naviam.Code;

namespace Naviam.Controllers
{
    public class AccountController : Controller
    {


        // **************************************
        // URL: /Account/LogOn
        // **************************************

        public ActionResult LogOn()
        {
            //remove from redis
            SessionHelper.UserProfile = null;
            FormsAuthentication.SignOut();
            Session.Abandon();
            //TestDataAdapter.Test();
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                UserProfile prof = UserDataAdapter.GetUserProfile(model.UserName, model.Password);
                //UserProfile prof = new UserProfile() { Id = 10};
                if (prof != null)
                {
                    string cId = Guid.NewGuid().ToString();
                    SessionHelper.SetNewUserProfile(cId, prof);
                    //setup forms ticket
                    //TODO: sliding expiration (need to add into redis logic too)
                    DateTime exp = DateTime.Now.Add(FormsAuthentication.Timeout);
                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, model.UserName, DateTime.Now, exp, model.RememberMe, cId);
                    HttpCookie fCookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
                    if (model.RememberMe)
                        fCookie.Expires = ticket.Expiration;
                    Response.Cookies.Add(fCookie);

                    //TODO: setup locale into Session["Culture"]

                    if (!String.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                        return RedirectToAction("Index", "Transactions");

                }
                ModelState.AddModelError("", "The user name or password provided is incorrect.");
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // **************************************
        // URL: /Account/LogOff
        // **************************************

        public ActionResult LogOff()
        {
            return RedirectToAction("LogOn");
        }

    }
}
