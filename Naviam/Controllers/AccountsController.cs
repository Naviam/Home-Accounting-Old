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
            //var accounts = Repository<Account>.GetList(AccountsDataAdapter.GetAccounts, new Dictionary<string, object>(){{"@id_company", user.CurrentCompany.ToDbValue()}}, user.CurrentCompany);
            var accounts = AccountsRepository.GetAccounts(user.CurrentCompany);
            var currencies = CurrenciesDataAdapter.GetCurrencies();
            var accauntTypes = AccountTypesDataAdapter.GetAccountTypes();
            return Json(new { items = accounts, currItems = currencies, typesItems = accauntTypes });
        }

        [HttpPost]
        public ActionResult UpdateAccount(Account account)
        {
            var user = CurrentUser;

            if (account.Id == null)
            {
                account.CompanyId = user.CurrentCompany;
                AccountsRepository.Insert(account,user.CurrentCompany);
            }
            else 
            {
                AccountsRepository.Update(account, user.CurrentCompany);
            }
            return Json(account);
        }

        [HttpPost]
        public ActionResult DeleteAccount(int? id)
        {
            var user = CurrentUser;
            Account acc = AccountsRepository.GetAccount(id, CurrentUser.CurrentCompany);
            AccountsRepository.Delete(acc, CurrentUser.CurrentCompany);
            return Json(id);        
        }
    }
}
