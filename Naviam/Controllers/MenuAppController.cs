using System.Web.Mvc;
using Naviam.WebUI.Models;

namespace Naviam.WebUI.Controllers
{

    public class MenuAppController : BaseController
    {
        public ActionResult TopMenu(string sitemapConfigName)
        {
            return PartialView(new MenuModel(GetController(), GetAction(), Request.QueryString));
        }

        public ActionResult SubMenu()
        {
            return PartialView(new MenuModel(GetController(), GetAction()));
        }

        public ActionResult CompaniesMenu()
        {
            ViewBag.CurrentCompany = CurrentUser.CurrentCompany;
            return PartialView(CurrentUser.Companies);
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
