using System.Web.Mvc;
using Naviam.DAL;
using Naviam.WebUI.Resources;
using System.Collections.Generic;

using Naviam.Data;
using Naviam.Domain.Concrete;
using System;

namespace Naviam.WebUI.Controllers
{
    public class AccountsController : BaseController
    {
        [Serializable]
        public class FinanceInstitution : DbEntity
        {
            public string Name { get; set; }
            public int? TypeId { get; set; }
        }

        [Serializable]
        public class FinanceInstitutionLinkToAccount
        {
            public int? FinanceTypeId { get; set; }
            public int? AccountTypeId { get; set; }
        }

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
            //TODO: translate account types
            //TODO: !!!!!!!create repository and get from it
            List<FinanceInstitution> finInst = new List<FinanceInstitution>();
            finInst.Add(new FinanceInstitution() { Id = 15, Name = "BelSwiss", TypeId = 2 });
            finInst.Add(new FinanceInstitution() { Id = 33, Name = "Home", TypeId = 5 });
            List<FinanceInstitutionLinkToAccount> finLinks = new List<FinanceInstitutionLinkToAccount>();
            finLinks.Add(new FinanceInstitutionLinkToAccount() { FinanceTypeId = 2, AccountTypeId = 2 });
            finLinks.Add(new FinanceInstitutionLinkToAccount() { FinanceTypeId = 2, AccountTypeId = 5 });
            finLinks.Add(new FinanceInstitutionLinkToAccount() { FinanceTypeId = 5, AccountTypeId = 1 });
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
