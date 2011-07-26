using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Naviam.Controllers
{
    public class BankAccountsDataController : BaseController
    {
        public ActionResult Accounts()
        {
            return View();
        }
    }
}
