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
using System.Data.SqlClient;
using Naviam.Data;
using log4net;

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
                ILog log = LogManager.GetLogger("navSite");
                log.Debug("LogOn");

                var profile = _membershipRepository.GetUser(model.UserName.ToLower(), model.Password);
                //var profile = new Data.UserProfile();

                if (profile != null)
                {
                    return AuthSuccess(profile, model, returnUrl, profile.IsApproved);
                }
                ModelState.AddModelError(String.Empty, ValidationStrings.UsernameOrPasswordIsIncorrect);
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private ActionResult AuthSuccess(Data.UserProfile profile, LogOnModel model, string returnUrl, bool isApprove)
        {
            if (!isApprove) return RedirectToAction("Confirmation", "Account", new { acc = profile.ApproveCode });
            //setup forms ticket
            var sessionKey = _membershipRepository.SetSessionForUser(profile);

            _cookieContainer.SetAuthCookie(sessionKey, model.UserName.ToLower(), model.RememberMe);

            Session["Culture"] = !String.IsNullOrEmpty(profile.LanguageNameShort) ? new CultureInfo(profile.LanguageNameShort) : null;
            // Make sure we only follow relative returnUrl parameters to protect against having an open redirector
            if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Transactions");
        }

        public ActionResult Confirmation(string acc)
        {
            var model = new ConfirmationModel();
            model.ApproveCode = acc;
            return View(model);
        }

        [HttpPost]
        public ActionResult Confirmation(ConfirmationModel model)
        {
            if (ModelState.IsValid)
            {
                UserProfile profile = null;
                profile = _membershipRepository.GetUserByApproveCode(model.ApproveCode);

                if (profile != null)
                {
                    profile.IsApproved = _membershipRepository.Approve(profile.Name);
                    return AuthSuccess(profile, new LogOnModel() { UserName = profile.Name, RememberMe = false }, "", true);
                }
                ModelState.AddModelError(String.Empty, ValidationStrings.UsernameOrPasswordIsIncorrect);
            }
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
                            LogOnModel model = new LogOnModel() { UserName = fResp.Attributes[WellKnownAttributes.Contact.Email].Values[0] };
                            var profile = _membershipRepository.GetUser(model.UserName.ToLower(), model.Password, true);

                            if (profile != null)
                                return AuthSuccess(profile, model, null, profile.IsApproved);
                            else
                                return Register();
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
            UserProfile profile = null;
            LogOnModel lModel = new LogOnModel() { UserName = model.UserName };

            if (ModelState.IsValid && model.Password == model.ConfirmPassword)
            {
                try
                {
                    profile = _membershipRepository.CreateUser(model.UserName, model.Password);
                    EmailHelper.SendMail("Confirmation of registration on naviam.com", model.UserName, string.Format(@"http://localhost:54345/Account/Confirmation?acc={0}", profile.ApproveCode), "alert@naviam.com");
                }
                catch (SqlException e)
                {
                    switch (e.Number)
                    {
                        case 50000:
                            profile = _membershipRepository.GetUser(model.UserName.ToLower(), model.Password, true);
                            return RedirectToAction("Confirmation", "Account", new { acc = profile.ApproveCode });
                        default:
                            ModelState.AddModelError(String.Empty, e.Message);
                            return View(model);
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(String.Empty, e.Message);
                    return View(model);
                }

                profile = _membershipRepository.GetUser(model.UserName.ToLower(), model.Password, true);
                if (profile != null)
                {
                    return AuthSuccess(profile, lModel, null, profile.IsApproved);
                }
            }

            //send mail with registration link
            //return View("Confirmation");
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
