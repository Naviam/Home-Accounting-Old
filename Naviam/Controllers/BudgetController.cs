﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Naviam.WebUI.Controllers
{
    public class BudgetController : BaseController
    {
        public ActionResult Budget()
        {
            return View();
        }

    }
}