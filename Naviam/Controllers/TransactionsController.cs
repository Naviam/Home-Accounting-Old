using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Naviam.Code;
using Naviam.Data;
using Naviam.DAL;

namespace Naviam.Controllers
{
    public class TransactionsController : BaseController
    {


        public class Paging
        {
            public int PageSize = 3;
            public int Page { get; set; }
            public int PagesCount { get; set; }
            public int RowsOnPage { get; set; }
            public int RowsCount { get; set; }
            public string Filter { get; set; }
            public string Sort { get; set; }

            public IEnumerable<T> ApplyPaging<T>(IEnumerable<T> input)
            {
                PagesCount = (int)Math.Ceiling((double)input.Count() / PageSize);
                RowsCount = input.Count();
                if (Page < 1) Page = 1;
                if (Page > PagesCount) Page = PagesCount;
                input = input.Skip((Page - 1) * PageSize).Take(PageSize).ToList();
                RowsOnPage = input.Count();
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
            UserProfile user = SessionHelper.UserProfile;
            IEnumerable<Transaction> trans = TransactionsDataAdapter.GetTransactions(user.Id);

            trans = paging.ApplyPaging<Transaction>(trans);

            return Json(new { items = trans, paging = paging });
        }

        [HttpPost]
        public string UpdateTransaction(Transaction trans)
        {
            UserProfile user = SessionHelper.UserProfile;
            //Transaction updateTrans = TransactionsDataAdapter.GetTransaction(trans.Id, user.Id);
            //TryUpdateModel(updateTrans);
            TransactionsDataAdapter.Update(trans, user.Id);
            return "ok";
        }

    }
}
