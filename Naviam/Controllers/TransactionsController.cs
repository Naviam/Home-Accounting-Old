using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using Naviam.Data;
using Naviam.DAL;
using Naviam.WebUI.Resources;

using Naviam.WebUI.Models;
using Naviam.Domain.Concrete;
using System.Resources;

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
            public int PageSize = 10;
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
            var trans = new TransactionsRepository().GetTransactions(user.CurrentCompany);
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
                               new Head {Field = "Merchant", Text = DisplayNames.Merchant},
                               new Head {Field = "CategoryId", Text = DisplayNames.Category},
                               new Head {Field = "Amount", Text = DisplayNames.Amount, Columns = 2}
                           };

            return Json(new { items = trans, paging, headItems = head, transTemplate = new Transaction() });
        }

        [HttpPost]
        public ActionResult UpdateTransaction(Transaction trans, PageContext pageContext)
        {
            var companyId = CurrentUser.CurrentCompany;
            var rep = new TransactionsRepository();
            //TryUpdateModel(updateTrans);
            var amount = trans.Amount;
            //if (pageContext.AccountId != null)
            //    trans.AccountId = pageContext.AccountId;
            if (trans.Id != null)
            {
                var updateTrans = TransactionsDataAdapter.GetTransaction(trans.Id, companyId);
                amount = (trans.Direction == updateTrans.Direction) ? -(updateTrans.Amount - trans.Amount) : trans.Amount * 2;
                rep.Update(trans, companyId);
            }
            else
                rep.Insert(trans, companyId);
            return Json(new { trans, amount });
        }

        [HttpPost]
        public ActionResult DeleteTransaction(int? id)
        {
            var user = CurrentUser;
            new TransactionsRepository().Delete(id, user.CurrentCompany);
            return Json(id);
        }

        [HttpPost]
        public ActionResult GetCategories()
        {
            var user = CurrentUser;
            var cats = CategoriesRepository.GetCategories(user.Id);
            //TODO: move Localize
            var rm = new ResourceManager(typeof(Resources.Enums));
            foreach (var item in cats)
            {
                var st = rm.GetString("c_" + item.Id.ToString());
                if (!String.IsNullOrEmpty(st))
                    item.Name = st;
            }
            //end Localize
            var items = Categories.GetTree(cats);
            return Json(new { items });
        }

        [HttpPost]
        public ActionResult GetCategoriesEditDialog()
        {
            return PartialView("_categoriesEdit");
        }

        [HttpPost]
        public ActionResult GetExchangeDialog()
        {
            return PartialView("_exchangeDialog");
        }

        [HttpPost]
        public ActionResult GetSplitDialog()
        {
            return PartialView("_splitDialog");
        }

        [HttpPost]
        public ActionResult SplitTrans(TransactionsSplit splits)
        {
            var companyId = CurrentUser.CurrentCompany;
            var updateTrans = TransactionsDataAdapter.GetTransaction(splits.Id, companyId);
            var rep = new TransactionsRepository();
            if (updateTrans != null)
            {
                updateTrans.Amount = splits.EndAmount;
                rep.Update(updateTrans, companyId);
                foreach (var item in splits.Items)
                {
                    updateTrans.Description = item.Description;
                    updateTrans.Merchant = item.Merchant;
                    updateTrans.CategoryId = item.CategoryId;
                    updateTrans.Amount = item.Amount;
                    rep.Insert(updateTrans, companyId);
                }
            }
            return Json(updateTrans);
        }

        [HttpPost]
        public ActionResult UpdateCategory(Category cat)
        {
            var user = CurrentUser;
            cat.UserId = user.Id;
            if (cat.Subitems.Count == 1 && cat.Subitems[0] == null)
                cat.Subitems.Clear();
            if (cat.Id != null)
                CategoriesRepository.Update(cat, cat.UserId);
            else
                CategoriesRepository.Insert(cat, cat.UserId);
            return Json(cat);
        }
        
        [HttpPost]
        public ActionResult DeleteCategory(int? id)
        {
            var user = CurrentUser;
            CategoriesRepository.Delete(id, user.Id);
            return Json(id);
        }

    }
}
