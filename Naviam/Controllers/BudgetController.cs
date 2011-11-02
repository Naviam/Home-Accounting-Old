using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Naviam.Domain.Concrete;

namespace Naviam.WebUI.Controllers
{
    public class Budget
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double Amount { get; set; }
    }

    public class GetBudgetRequest
    {
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
    }

    public class BudgetController : BaseController
    {
        private readonly BudgetRepository _budgetRepository;

        public BudgetController(BudgetRepository budgetRepository = null)
        {
            _budgetRepository = budgetRepository ?? new BudgetRepository();
        }

        [HttpPost]
        public ActionResult BudgetGet(GetBudgetRequest request)
        {
            var user = CurrentUser;
            var initialState = _budgetRepository.GetBudgets(user.CurrentCompany);
            return Json(new {items = initialState });
            //return View(initialState);
            //return View();
        }

        public ActionResult Budget()
        {
            return View();
        }



    }
}
