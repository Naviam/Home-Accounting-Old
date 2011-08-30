using System.Web.Mvc;
using Naviam.DAL;
using Naviam.WebUI.Resources;
using System.Collections.Generic;

using Naviam.Data;
using Naviam.Domain.Concrete;
using System;
using System.Resources;

namespace Naviam.WebUI.Controllers
{
    public class AccountsController : BaseController
    {

        public ActionResult Index()
        {
            return View("Accounts");
        }

        [HttpPost]
        public ActionResult GetAccounts()
        {
            var user = CurrentUser;
            var accounts = AccountsRepository.GetAccounts(user.CurrentCompany);
            var currencies = CurrenciesRepository.GetCurrencies();
            var accountTypes = AccountTypesRepository.GetAccountTypes();
            //TODO: move Localize
            var rm = new ResourceManager(typeof(Resources.Enums));
            foreach (var item in accountTypes)
            {
                var st = rm.GetString("acct_" + item.Id.ToString());
                if (!String.IsNullOrEmpty(st))
                    item.TypeName = st;
            }
            //end Localize
            var finInst = FinanceInstitutionRepository.Get();
            var finLinks = FinanceInstitutionRepository.GetLinksToAccount();
            foreach (var account in accounts)
            {
                account.Currency = currencies.Find(c => c.Id == account.CurrencyId).NameShort;
            }
            return Json(new { items = accounts, currItems = currencies, typesItems = accountTypes, finInst, finLinks });
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
            var currencies = CurrenciesDataAdapter.GetCurrencies();
            account.Currency = currencies.Find(c => c.Id == account.CurrencyId).NameShort;
            return Json(account);
        }

        [HttpPost]
        public ActionResult DeleteAccount(int? id)
        {
            var companyId = CurrentUser.CurrentCompany;
            Account acc = AccountsRepository.GetAccount(id, companyId);
            var res = AccountsRepository.Delete(acc, companyId);
            if (res == 0)
                new TransactionsRepository().ResetCache(companyId);
            return Json(id);        
        }

        [HttpPost]
        public ActionResult AddAccountAmount(int? id, decimal amount)
        {
            var companyId = CurrentUser.CurrentCompany;
            AccountsRepository.ChangeBalance(id, companyId, amount);
            /*Account acc = AccountsRepository.GetAccount(id, companyId);
            acc.Balance = acc.Balance + amount;
            AccountsRepository.Update(acc, companyId);*/
            return Json(id);
        }
    }
}
