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
                //UserProfile prof = new UserProfile();
                if (prof != null)
                {
                    SessionHelper.UserProfile = prof;
                    //setup forms ticket
                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(model.UserName, false, (int)FormsAuthentication.Timeout.TotalMinutes);
                    Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)));

                    //TODO: setup locale into Session["Culture"]

                    if (!String.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                        return RedirectToAction("Accounts", "BankAccountsData");

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
