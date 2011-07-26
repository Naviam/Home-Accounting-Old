﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Naviam.Models;

namespace Naviam.Controllers
{

    public class MenuAppController : Controller
    {
        public ActionResult TopMenu()
        {
            return PartialView(new MenuModel(GetController(), GetAction()));
        }

        public ActionResult SubMenu()
        {
            return PartialView(new MenuModel(GetController(), GetAction()));
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
