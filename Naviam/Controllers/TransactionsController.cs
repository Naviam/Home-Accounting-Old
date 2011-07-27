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

        private const int PageSize = 3;

        public class Paging
        {
            public int Page { get; set; }
            public int PagesCount { get; set; }
            public int RowsOnPage { get; set; }
            public int RowsCount { get; set; }
            public string Filter { get; set; }
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
            
            paging.PagesCount = (int)Math.Ceiling((double)trans.Count() / PageSize);
            paging.RowsCount = trans.Count();
            if (paging.Page < 1) paging.Page = 1;
            if (paging.Page > paging.PagesCount) paging.Page = paging.PagesCount;
            trans = trans.Skip((paging.Page - 1) * PageSize).Take(PageSize).ToList();
            paging.RowsOnPage = trans.Count();

            return Json(new { items = trans, paging = paging });
        }

    }
}
