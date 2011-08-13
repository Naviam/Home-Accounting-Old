using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Naviam.Domain.Abstract;
using Naviam.WebUI.Helpers;
using Naviam.WebUI.Models;

namespace Naviam.WebUI.Controllers
{
    [HandleErrorWithElmah]
    public class AccountController : Controller
    {
        private readonly IMembershipRepository _membershipRepository;

        public AccountController(IMembershipRepository membershipRepository,
            IFormsAuthentication formsAuth, IMembershipService service)
        {
            _membershipRepository = membershipRepository;
            FormsAuth = formsAuth ?? new FormsAuthenticationService();
            MembershipService = service ?? new AccountMembershipService();
        }

        public IFormsAuthentication FormsAuth { get; private set; }

        public IMembershipService MembershipService { get; private set; }

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
                //var profile = MembershipDataAdapter.GetUserProfile(model.UserName, model.Password);
                //UserProfile prof = new UserProfile() { Id = 10};
                var profile = _membershipRepository.GetUserProfile(model.UserName, model.Password);
                
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

    public interface IFormsAuthentication
    {
        void SignIn(string userName, bool createPersistentCookie);
        void SignOut();
    }

    public class FormsAuthenticationService : IFormsAuthentication
    {
        public void SignIn(string userName, bool createPersistentCookie)
        {
            FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
        }
        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }

    public interface IMembershipService
    {
        int MinPasswordLength { get; }

        bool ValidateUser(string userName, string password);
        string GetCanonicalUsername(string userName);
        MembershipCreateStatus CreateUser(string userName, string password, string email);
        bool ChangePassword(string userName, string oldPassword, string newPassword);
    }

    public class AccountMembershipService : IMembershipService
    {
        private readonly MembershipProvider _provider;

        public AccountMembershipService()
            : this(null)
        {
        }

        public AccountMembershipService(MembershipProvider provider)
        {
            _provider = provider ?? Membership.Provider;
        }

        public int MinPasswordLength
        {
            get
            {
                return _provider.MinRequiredPasswordLength;
            }
        }

        public bool ValidateUser(string userName, string password)
        {
            return _provider.ValidateUser(userName, password);
        }

        public string GetCanonicalUsername(string userName)
        {
            var user = _provider.GetUser(userName, true);
            return user != null ? user.UserName : null;
        }

        public MembershipCreateStatus CreateUser(string userName, string password, string email)
        {
            MembershipCreateStatus status;
            _provider.CreateUser(userName, password, email, null, null, true, null, out status);
            return status;
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            var currentUser = _provider.GetUser(userName, true /* userIsOnline */);
            return currentUser != null && currentUser.ChangePassword(oldPassword, newPassword);
        }
    }
}
