using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Naviam.Domain.Concrete;

using Naviam.Data;

namespace Naviam.WebUI.Controllers
{
    public class ReportsController : BaseController
    {
        public class GetReportRequest
        {
            public Currency selectedCurrency;
        }

        private readonly TransactionsRepository _transRepository;
        private readonly CategoriesRepository _categoriesRepository;
        private readonly CurrenciesRepository _currenciesRepository;


        public ReportsController(TransactionsRepository transRepository = null, CategoriesRepository categoriesRepository = null, CurrenciesRepository currenciesRepository = null)
        {
            _transRepository = transRepository ?? new TransactionsRepository();
            _categoriesRepository = categoriesRepository ?? new CategoriesRepository();
            _currenciesRepository = currenciesRepository ?? new CurrenciesRepository();
        }

        public ActionResult Reports()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetReport(int? selectedCurrency)
        {
            var user = CurrentUser;
            var trans = _transRepository.GetTransactions(user.CurrentCompany);
            var cats = _categoriesRepository.GetAll(user.Id);
            var a_currencies = _currenciesRepository.GetCurrencies();

            var currencies =  from t in trans
                          join c in a_currencies on t.CurrencyId equals c.Id
                          where t.Direction == TransactionDirections.Expense
                          group t by c into g
                          select new { Id = g.Key.Id, NameShort = g.Key.NameShort };

            int? currencyId = selectedCurrency ?? currencies.FirstOrDefault().Id;

            var report = from t in trans 
                         join c in cats on t.CategoryId equals c.Id
                         where t.Direction == TransactionDirections.Expense && t.CurrencyId == currencyId
                         group t.Amount by c into g
                         orderby g.Key.Name
                         select new { CategoryId = g.Key.Id, Amount = g.Sum(), CategoryName = g.Key.Name }; //

            var request = new GetReportRequest();
            request.selectedCurrency = new Currency() {Id=1};

            return Json(new { items = report, currencies, currencyId });
        }

    }
}
