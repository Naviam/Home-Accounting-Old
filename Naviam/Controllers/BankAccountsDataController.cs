using System.Web.Mvc;
using Naviam.DAL;
using Naviam.WebUI.Resources;
using System.Collections.Generic;

namespace Naviam.WebUI.Controllers
{
    public class BankAccountsDataController : BaseController
    {
        public ActionResult Index()
        {
            return View("Accounts");
        }

        [HttpPost]
        public ActionResult GetAccounts()
        {
            var user = CurrentUser;
            var accounts = AccountsDataAdapter.GetAccounts(user.CurrentCompany, user.LanguageId, false);
            var head = new List<Naviam.WebUI.Controllers.TransactionsController.Head>
                           {
                               new Naviam.WebUI.Controllers.TransactionsController.Head {Field = "DateCreation", Text = DisplayNames.Date},
                               new Naviam.WebUI.Controllers.TransactionsController.Head {Field = "Number", Text = DisplayNames.Account},
                               new Naviam.WebUI.Controllers.TransactionsController.Head {Field = "Balance", Text = DisplayNames.Balance},
                               new Naviam.WebUI.Controllers.TransactionsController.Head {Field = "Currency", Text = DisplayNames.Currency},
                               new Naviam.WebUI.Controllers.TransactionsController.Head {Field = "TypeName", Text = DisplayNames.AccountType},
                           };

            return Json(new { items = accounts, headItems = head });
        }
    }
}
