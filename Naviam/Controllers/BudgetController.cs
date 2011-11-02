using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Naviam.WebUI.Controllers
{
    public class Budget
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double Amount { get; set; }
    }


    public class BudgetController : BaseController
    {
        [HttpPost]
        public ActionResult BudgetGet()
        {
            var initialState = new List<Budget> {
                new Budget { Id = 1, Title = "Tall Hat", Amount = 49.95 },
                new Budget { Id = 2, Title = "Long Cloak", Amount = 78.25 }
            };
            return Json(new { initialState });
            //return View(initialState);
            //return View();
        }

        public ActionResult Budget()
        {
            var initialState = new List<Budget> {
                new Budget { Id = 1, Title = "Tall Hat", Amount = 49.95 },
                new Budget { Id = 2, Title = "Long Cloak", Amount = 78.25 }
            };
            return Json(new { initialState });
            //return View();
        }



    }
}
