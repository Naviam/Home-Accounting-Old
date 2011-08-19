using System.Web.Mvc;
using Naviam.DAL;
using Naviam.WebUI.Resources;
using System.Collections.Generic;

using Naviam.Data;
using Naviam.Domain.Concrete;

namespace Naviam.WebUI.Controllers
{
    public class AccountsController : BaseController
    {
        public ActionResult Index()
        {
            return View("Accounts");
        }

        //[HttpPost]
        //public ActionResult GetAccounts()
        //{
        //    var user = CurrentUser;
        //    var accounts = AccountsDataAdapter.GetAccounts(user.CurrentCompany, user.LanguageId, false);
        //    var head = new List<TransactionsController.Head>
        //                   {
        //                       new TransactionsController.Head {Field = "DateCreation", Text = DisplayNames.Date},
        //                       new TransactionsController.Head {Field = "Number", Text = DisplayNames.Account},
        //                       new TransactionsController.Head {Field = "Balance", Text = DisplayNames.Balance},
        //                       new TransactionsController.Head {Field = "Currency", Text = DisplayNames.Currency},
        //                       new TransactionsController.Head {Field = "TypeName", Text = DisplayNames.AccountType},
        //                   };

        //    return Json(new { items = accounts, headItems = head });
        //}

        [HttpPost]
        public ActionResult GetAccounts()
        {
            var user = CurrentUser;
            var accounts = Repository<Account>.GetList(AccountsDataAdapter.GetAccounts, new Dictionary<string, object>(){{"@id_company", user.CurrentCompany.ToDbValue()}}, user.CurrentCompany);
            //var transactions = Repository<Transaction>.GetList(TransactionsDataAdapter.GetTransactions, new Dictionary<string, object>() { { "@id_company", user.CurrentCompany.ToDbValue() } }, user.CurrentCompany);
            //var accounts = AccountsDataAdapter.GetAccounts(user.CurrentCompany);
            var currencies = CurrenciesDataAdapter.GetCurrencies();
            //accounts.Insert(0, new Account() { Number = "All" });
            return Json(new { items = accounts, currItems = currencies });
        }

        [HttpPost]
        public ActionResult UpdateAccount(Account account)
        {
            //UserProfile usr = new UserProfile();
            //usr.Name = "lingM@tut.by";
            //usr.Password = "5QP1nZZ9syOu+Hr0zNTbMEgplM9gKxH5wpTpQyXH4lggYme3gbWtlLoqAeM5xdkAwVpdaXDVG5VIAyD3UR66CbcHA1Sp";
            //MembershipDataAdapter.CreateUser(usr);
            return Json(account);
        }

        [HttpPost]
        public string DeleteAccount(int? id)
        {

            return "ok";        
        }
    }
}
