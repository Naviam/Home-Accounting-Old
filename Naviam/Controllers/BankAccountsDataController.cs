using System.Web.Mvc;

namespace Naviam.WebUI.Controllers
{
    public class BankAccountsDataController : BaseController
    {
        public ActionResult Index()
        {
            return View("Accounts");
        }
    }
}
