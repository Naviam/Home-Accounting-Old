﻿using System.Web.Mvc;
using Naviam.DAL;
using Naviam.WebUI.Resources;
using System.Collections.Generic;

using Naviam.Data;

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
            var accounts = AccountsDataAdapter.GetAccounts(user.CurrentCompany, user.LanguageId, false);
            var head = new List<TransactionsController.Head>
                           {
                               new TransactionsController.Head {Field = "DateCreation", Text = DisplayNames.Date},
                               new TransactionsController.Head {Field = "Number", Text = DisplayNames.Account},
                               new TransactionsController.Head {Field = "Balance", Text = DisplayNames.Balance},
                               new TransactionsController.Head {Field = "Currency", Text = DisplayNames.Currency},
                               new TransactionsController.Head {Field = "TypeName", Text = DisplayNames.AccountType},
                           };

            return Json(new { items = accounts, headItems = head });
        }

        [HttpPost]
        public ActionResult UpdateAccount(Account account)
        {
            return Json(account);
        }
    }
}
