using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Naviam.WebUI.Models;

namespace Naviam.WebUI.Controllers
{
    public class PublicMenuController : Controller
    {
        public ActionResult TopMenu(string sitemapConfigName)
        {
            var model = new MenuModel(GetController(), GetAction(), Request.QueryString, "publicSiteMapFile");
            return PartialView(model);
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
