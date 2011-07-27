using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Naviam.Controllers
{
    public class TransactionsController : BaseController
    {
        class Transaction
        {
            public string Description { get; set; }
            public string Category { get; set; }
            public int Amount { get; set; }
        }
        
        public class Paging
        {
            public int Page { get; set; }
            public int PagesCount { get; set; }
            public int RowsPerPage { get; set; }
            public string Filter { get; set; }
        }


        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetTransactions(Paging paging)
        {
            List<Transaction> trans = new List<Transaction>();
            if (paging.Page == 1)
            {
                trans.Add(new Transaction() { Description = "Test", Category = "Dinner", Amount = 100 });
                trans.Add(new Transaction() { Description = "Test", Category = "Dinner", Amount = 100 });
                trans.Add(new Transaction() { Description = "Test", Category = "Dinner", Amount = 100 });
                trans.Add(new Transaction() { Description = "Test", Category = "Dinner", Amount = 100 });
            }
            else
            {
                trans.Add(new Transaction() { Description = "Test2", Category = "Dinner2", Amount = 120 });
                trans.Add(new Transaction() { Description = "Test2", Category = "Dinner2", Amount = 120 });
                trans.Add(new Transaction() { Description = "Test2", Category = "Dinner2", Amount = 120 });
            }
            paging.PagesCount = 10;
            paging.RowsPerPage = 5;
            return Json(new { items = trans, paging = paging });
        }

    }
}
