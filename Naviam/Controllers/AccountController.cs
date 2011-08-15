﻿using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Naviam.Domain.Concrete;
using Naviam.WebUI.Helpers;
using Naviam.WebUI.Helpers.Cookies;
using Naviam.WebUI.Models;

namespace Naviam.WebUI.Controllers
{
    [HandleErrorWithElmah]
    public class AccountController : Controller
    {
        private readonly MembershipRepository _membershipRepository;
        private readonly ICacheWrapper _cacheWrapper;
        private readonly ICookieContainer _cookieContainer;

        // This constructor is used by the MVC framework to instantiate the controller using
        // the default forms authentication and membership providers.

        public AccountController()
            : this(null, null, null, new MembershipRepository())
        {
        }

        public AccountController(ICookieContainer cookieContainer, IFormsAuthentication formsAuth, 
            ICacheWrapper cacheWrapper, MembershipRepository membershipRepository)
        {
            _cookieContainer = cookieContainer;
            _membershipRepository = membershipRepository;
            _cacheWrapper = cacheWrapper;
            FormsAuth = formsAuth ?? new FormsAuthenticationService();
        }

        public IFormsAuthentication FormsAuth { get; private set; }

        // **************************************
        // URL: /Account/LogOn
        // **************************************

        public ActionResult LogOn()
        {
            //remove from redis
            SessionHelper.UserProfile = null;
            if (User != null && User.Identity.IsAuthenticated)
                FormsAuth.SignOut();
            if (Session != null) Session.Abandon();
            return View("LogOn");
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var profile = _membershipRepository.GetUser(model.UserName.ToLower(), model.Password);

                if (profile != null)
                {
                    //setup forms ticket
                    var sessionKey = _membershipRepository.SetSessionForUser(profile);

                    _cookieContainer.SetAuthCookie(
                        sessionKey, model.UserName.ToLower(), model.RememberMe);

                    //TODO: setup locale into Session["Culture"]
                    // Make sure we only follow relative returnUrl parameters to protect against having an open redirector
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Transactions");
                }
                ModelState.AddModelError("", @"The user name or password provided is incorrect.");
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // **************************************
        // URL: /Account/LogOff
        // **************************************

        public ActionResult LogOff()
        {
            return RedirectToAction("LogOn", "Account");
        }

        public ActionResult Register()
        {
            return View("Register");
        }

        [HttpPost]
        public ActionResult Register(string email, string password, string confirmPassword)
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string email, string captcha)
        {
            return View("LogOn");
        }
    }

    // The FormsAuthentication type is sealed and contains static members, so it is difficult to
    // unit test code that calls its members. The interface and helper class below demonstrate
    // how to create an abstract wrapper around such a type in order to make the AccountController
    // code unit testable.

    public interface IFormsAuthentication
    {
        void SignIn(string userName, bool createPersistentCookie);
        void SignOut();
    }

    public class FormsAuthenticationService : IFormsAuthentication
    {
        public void SignIn(string userName, bool createPersistentCookie)
        {
            var authTicket = new FormsAuthenticationTicket(
                1, //version
                userName, // user name
                DateTime.Now,             //creation
                DateTime.Now.AddMinutes(30), //Expiration
                createPersistentCookie, //Persistent
                userName); //since Classic logins don't have a "Friendly Name".  OpenID logins are handled in the AuthController.

            var encTicket = FormsAuthentication.Encrypt(authTicket);
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
        }
        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }
}
