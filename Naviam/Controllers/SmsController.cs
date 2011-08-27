using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Naviam.WebUI.Controllers
{
    public class SmsController : Controller
    {

        [HttpPost]
        public ActionResult RecieveMessage(string key, string gateway, string from, string to, string message)
        {
            //throw new Exception("ddd");
            return Json("ok");
        }

    }
}
