using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Naviam.Domain.Concrete;
using Naviam.WebUI.Helpers;
using Naviam.WebUI.Helpers.Cookies;
using Naviam.WebUI.Models;
using Naviam.WebUI.Resources;
using System.Globalization;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;

namespace Naviam.WebUI.Controllers
{
    [HandleErrorWithElmah]
    public class AccountController : Controller
    {
        private readonly MembershipRepository _membershipRepository;
        private readonly ICookieContainer _cookieContainer;

        // This constructor is used by the MVC framework to instantiate the controller using
        // the default forms authentication and membership providers.

        public AccountController()
            : this(null, null, new MembershipRepository())
        {
        }

        public AccountController(ICookieContainer cookieContainer, IFormsAuthentication formsAuth, MembershipRepository membershipRepository)
        {
            _cookieContainer = cookieContainer;
            _membershipRepository = membershipRepository;
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

                    _cookieContainer.SetAuthCookie(sessionKey, model.UserName.ToLower(), model.RememberMe);

                    Session["Culture"] = !String.IsNullOrEmpty(profile.LanguageNameShort) ? new CultureInfo(profile.LanguageNameShort) : null;
                    // Make sure we only follow relative returnUrl parameters to protect against having an open redirector
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Transactions");
                }
                ModelState.AddModelError(String.Empty, ValidationStrings.UsernameOrPasswordIsIncorrect);
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        public ActionResult GoogleLogin()
        {
            var openId = new OpenIdRelyingParty();
            var response = openId.GetResponse();
            // If we have no response, start
            if (response == null)
            {
                // Create a request and redirect the user
                var authReq = openId.CreateRequest("https://www.google.com/accounts/o8/id");
                var fetch = new FetchRequest();
                fetch.Attributes.AddRequired(WellKnownAttributes.Contact.Email);
                fetch.Attributes.AddOptional(WellKnownAttributes.Name.FullName);
                authReq.AddExtension(fetch);
                authReq.RedirectToProvider();
                return null;
            }
            else
            {
                var errorMessage = "Success";
                // We got a response - check it's valid
                switch (response.Status)
                {
                    case AuthenticationStatus.Authenticated:
                        {
                            var fResp = response.GetExtension<FetchResponse>();
                            //TODO: login or register
                        }
                        break;
                    case AuthenticationStatus.Canceled:
                        errorMessage = "Login was cancelled at the provider.";
                        break;
                    case AuthenticationStatus.Failed:
                        errorMessage = "Login failed at the provider.";
                        break;
                    case AuthenticationStatus.SetupRequired:
                        errorMessage = "The provider requires setup.";
                        break;
                    default:
                        errorMessage = "Login failed.";
                        break;
                }
                return Content(errorMessage);
            }
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
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("Index", "Transactions");
            }
            return View(model);
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
            var exp = DateTime.Now.Add(FormsAuthentication.Timeout);
            var authTicket = new FormsAuthenticationTicket(
                1, //version
                userName, // user name
                DateTime.Now,             //creation
                exp, //Expiration
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
