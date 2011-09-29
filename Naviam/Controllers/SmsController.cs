using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Naviam.Domain.Concrete;
using Naviam.Data;

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
        static string other = @"
4..0692 
Service payment from card 
Uspeshno 
2011-09-01 00:00:00 
Summa: 4828 BYR 
Ostatok: 56552 BYR 
Na vremya: 11:11:46 
//RBS Balance loader
4..0692 
Service payment to card 
Uspeshno 
2011-09-01 00:00:00 
Summa: 999999 BYR 
Ostatok: 1056551 BYR 
Na vremya: 16:26:28 
//RBS Balance loader
";
        [HttpPost]
        public ActionResult RecieveMessage(string key, string gateway, string from, string to, string message)
        {
            string internalKey = "";
            if (key != internalKey) return Json("ok");

            try
            {
                BelSwissSms sms = new BelSwissSms(message);
                
                TransactionsRepository rep = new TransactionsRepository();
                Transaction tran = new Transaction();
                //tran.AccountId AccountId = 
                //rep.Insert(
            }
            catch(Exception e)
            {
                throw e;
            }
            

            //throw new Exception("ddd");
            return Json("ok");
        }

    }
}
