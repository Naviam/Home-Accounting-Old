using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

using Naviam.Code;
using Naviam.Data;
using Naviam.Resources;
using Naviam.DAL;

namespace Naviam.Controllers
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
                    IQueryable<T> q = null;
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
        public ActionResult GetTransactions(Paging paging)
        {
            UserProfile user = CurrentUser;
            IEnumerable<Transaction> trans = TransactionsDataAdapter.GetTransactions(user.Id);

            trans = paging.ApplyPaging<Transaction>(trans);

            List<Head> head = new List<Head>();
            head.Add(new Head() { Field = "Date", Text = DisplayNames.Date});
            head.Add(new Head() { Field = "Description", Text = DisplayNames.Description });
            head.Add(new Head() { Field = "Category", Text = DisplayNames.Category });
            head.Add(new Head() { Field = "Amount", Text = DisplayNames.Amount, Columns = 2 });

            return Json(new { items = trans, paging = paging, headItems = head });
        }

        [HttpPost]
        public ActionResult UpdateTransaction(Transaction trans)
        {
            UserProfile user = CurrentUser;
            //Transaction updateTrans = TransactionsDataAdapter.GetTransaction(trans.Id, user.Id);
            //TryUpdateModel(updateTrans);
            if (trans.Id != null)
                TransactionsDataAdapter.Update(trans, user.Id);
            else
                TransactionsDataAdapter.Insert(trans, user.Id);
            return Json(trans);
        }
        
        [HttpPost]
        public ActionResult GetCategories()
        {
            UserProfile user = CurrentUser;

            IEnumerable<Category> items = CategoriesDataAdapter.GetCategories(user.Id);

            //List<Category> items = new List<Category>();
            //Category cat = new Category() { Name = "Food", Id = 1 };
            //cat.Subitems.Add(new Category() { Name = "Milk", Id = 2 });
            //cat.Subitems.Add(new Category() { Name = "Meat", Id = 3 });
            //items.Add(cat);
            //cat = new Category() { Name = "Auto", Id = 4 };
            //cat.Subitems.Add(new Category() { Name = "Oil", Id = 5 });
            //cat.Subitems.Add(new Category() { Name = "Gas", Id = 6 });
            //cat.Subitems.Add(new Category() { Name = "Spares", Id = 7 });
            //items.Add(cat);

            return Json(new { items = items });
        }

    }
}
