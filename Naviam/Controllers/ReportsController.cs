using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Naviam.Domain.Concrete;

namespace Naviam.WebUI.Controllers
{
    public class ReportsController : BaseController
    {

        private readonly TransactionsRepository _transRepository;
        private readonly CategoriesRepository _categoriesRepository;


        public ReportsController(TransactionsRepository transRepository = null, CategoriesRepository categoriesRepository = null)
        {
            _transRepository = transRepository ?? new TransactionsRepository();
            _categoriesRepository = categoriesRepository ?? new CategoriesRepository();
        }

        public ActionResult Reports()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetReport()
        {
            var user = CurrentUser;
            var trans = _transRepository.GetTransactions(user.CurrentCompany);
            var cats = _categoriesRepository.GetAll(user.Id);
            var report = from t in trans 
                         join c in cats on t.CategoryId equals c.Id
                         group t.Amount by c into g
                         orderby g.Key.Name
                         select new { Category = g.Key.Id, Amount = g.Sum(), CategoryName = g.Key.Name }; //

            return Json(new { items = report });
        }

    }
}
