using System.Web.Mvc;
using Naviam.WebUI.Models;

namespace Naviam.WebUI.Controllers
{

    public class MenuAppController : BaseController
    {
        public ActionResult TopMenu()
        {
            return PartialView(new MenuModel(GetController(), GetAction(), Request.QueryString));
        }

        public ActionResult SubMenu()
        {
            return PartialView(new MenuModel(GetController(), GetAction()));
        }

        public ActionResult CompaniesMenu()
        {
            var user = CurrentUser;
            ViewBag.DefaultCompany = user.DefaultCompany;
            return PartialView(user.Companies);
        }

        private string GetController()
        {
            string controller = string.Empty;
            if (Request.RequestContext.RouteData.Values["controller"] != null)
            {
                controller = Request.RequestContext.RouteData.Values["controller"].ToString();
            }
            return controller;
        }

        private string GetAction()
        {
            string action = string.Empty;
            if (Request.RequestContext.RouteData.Values["action"] != null)
            {
                action = Request.RequestContext.RouteData.Values["action"].ToString();
            }
            return action;
        }
    }
}
