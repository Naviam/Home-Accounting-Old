using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Naviam.Domain.Abstract;
using Naviam.WebUI.Helpers;
using Naviam.WebUI.Models;

namespace Naviam.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserAccountRepository _userAccountRepository;

        public AccountController(IUserAccountRepository userAccountRepository)
        {
            _userAccountRepository = userAccountRepository;
        }

        private void SignOut()
        {
            SessionHelper.UserProfile = null;
            if (User != null && User.Identity.IsAuthenticated)
                FormsAuthentication.SignOut();
            if (Session != null)
                Session.Abandon();
        }

        // **************************************
        // URL: /Account/LogOn
        // **************************************

        public ActionResult LogOn()
        {
            //remove from redis
            SignOut();
            return View("LogOn");
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                //var profile = UserDataAdapter.GetUserProfile(model.UserName, model.Password);
                //UserProfile prof = new UserProfile() { Id = 10};
                var profile = _userAccountRepository.GetUserProfile(model.UserName, model.Password);
                
                if (profile != null)
                {
                    var cId = Guid.NewGuid().ToString();
                    SessionHelper.SetNewUserProfile(cId, profile);
                    //setup forms ticket
                    var exp = DateTime.Now.Add(FormsAuthentication.Timeout);
                    var ticket = new FormsAuthenticationTicket(1, model.UserName, DateTime.Now, exp, model.RememberMe, cId);
                    var fCookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
                    if (model.RememberMe)
                        fCookie.Expires = ticket.Expiration;
                    Response.Cookies.Add(fCookie);

                    //TODO: setup locale into Session["Culture"]

                    if (!String.IsNullOrEmpty(returnUrl))
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
            SignOut();
            return View("LogOn");
        }

        public ActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SignIn(string email, string password)
        {
            return View();
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string email, string captcha)
        {
            return View();
        }
    }
}
