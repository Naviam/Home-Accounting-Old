using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Naviam.WebUI.Controllers
{
    public class SmsController : Controller
    {
        static string testMessage = @"
4..0692
Retail
Uspeshno
2011-08-22 09:32:37
Summa: 50000 BYR
Ostatok: 141380 BYR
Na vremya: 09:32:43
BLR/MINSK/BELCEL I-BANK
";
        [HttpPost]
        public ActionResult RecieveMessage(string key, string gateway, string from, string to, string message)
        {
            //throw new Exception("ddd");
            return Json("ok");
        }

    }
}
