using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using Naviam.Data;
using Naviam.DAL;
using Naviam.WebUI.Resources;

using Naviam.WebUI.Models;

namespace Naviam.WebUI.Controllers
{
    public class TransactionsController : BaseController
    {

        public class Head
        {
            public string Field { get; set; }
            public string Text { get; set; }
            public int Columns = 1;
        }

        public class Paging
        {
            public int PageSize = 3;
            public int Page { get; set; }
            public int PagesCount { get; set; }
            public int RowsOnPage { get; set; }
            public int RowsCount { get; set; }
            public string Filter { get; set; }
            public string SortField { get; set; }
            public int SortDirection { get; set; }

            public IEnumerable<T> ApplyPaging<T>(IEnumerable<T> input)
            {
                if (input != null)
                {
                    IQueryable<T> q;
                    if (!String.IsNullOrEmpty(Filter) && Filter != "null")
                    {
                        q = input.AsQueryable();
                        input = q.Where(Filter).ToList();
                    }
                    if (!String.IsNullOrEmpty(SortField) && SortField != "null")
                    {
                        string sort = SortField;
                        q = input.AsQueryable();
                        if (SortDirection != 0)
                            sort = sort + " desc";
                        input = q.OrderBy(sort).ToList();
                    }
                    PagesCount = (int)Math.Ceiling((double)input.Count() / PageSize);
                    RowsCount = input.Count();
                    if (Page < 1) Page = 1;
                    if (Page > PagesCount) Page = PagesCount;
                    input = input.Skip((Page - 1) * PageSize).Take(PageSize).ToList();
                    RowsOnPage = input.Count();
                }
                return input;
            }
        }


        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetTransactions(Paging paging, PageContext pageContext)
        {
            var user = CurrentUser;
            var trans = TransactionsDataAdapter.GetTransactions(user.CurrentCompany, user.LanguageId, false);
            paging.Filter = "";
            if (pageContext.AccountId != null)
            {
                paging.Filter = String.Format("AccountId={0}", pageContext.AccountId);
            }
            //var trans = TransactionsDataAdapter.GetTransactions(user.CurrentCompany);

            trans = paging.ApplyPaging(trans);

            var head = new List<Head>
                           {
                               new Head {Field = "Date", Text = DisplayNames.Date},
                               new Head {Field = "Description", Text = DisplayNames.Description},
                               new Head {Field = "Category", Text = DisplayNames.Category},
                               new Head {Field = "Amount", Text = DisplayNames.Amount, Columns = 2}
                           };

            return Json(new { items = trans, paging, headItems = head });
        }

        [HttpPost]
        public ActionResult UpdateTransaction(Transaction trans, PageContext pageContext)
        {
            var user = CurrentUser;
            //Transaction updateTrans = TransactionsDataAdapter.GetTransaction(trans.Id, user.Id);
            //TryUpdateModel(updateTrans);
            if (pageContext.AccountId != null)
                trans.AccountId = pageContext.AccountId;
            if (trans.Id != null)
                TransactionsDataAdapter.Update(trans, user.CurrentCompany, user.LanguageId);
            else
                TransactionsDataAdapter.Insert(trans, user.CurrentCompany, user.LanguageId);
            return Json(trans);
        }

        [HttpPost]
        public ActionResult DeleteTransaction(int? id)
        {
            var user = CurrentUser;
            var trans = TransactionsDataAdapter.GetTransaction(id, user.Id, user.LanguageId, false);
            TransactionsDataAdapter.Delete(trans, user.CurrentCompany, user.LanguageId);
            return Json(id);
        }

        [HttpPost]
        public ActionResult GetCategories()
        {
            var user = CurrentUser;
            var items = Categories.GetTree(CategoriesDataAdapter.GetCategories(user.Id));
            return Json(new { items });
        }

        [HttpPost]
        public ActionResult GetAccounts()
        {
            var user = CurrentUser;
            var accounts = AccountsDataAdapter.GetAccounts(user.CurrentCompany, user.LanguageId, false);
            accounts.Insert(0, new Account() { Number = "All"});
            return Json(new { items = accounts });
        }

    }
}
