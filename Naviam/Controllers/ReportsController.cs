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
            public int? selectedCurrency;
            public int selectedMenu;
            public int selectedSubMenu;
        }

        private readonly TransactionsRepository _transRepository;
        private readonly CategoriesRepository _categoriesRepository;
        private readonly CurrenciesRepository _currenciesRepository;
        private readonly TagsRepository _tagsRepository;


        public ReportsController(TransactionsRepository transRepository = null, CategoriesRepository categoriesRepository = null, CurrenciesRepository currenciesRepository = null, TagsRepository tagsRepository = null)
        {
            _transRepository = transRepository ?? new TransactionsRepository();
            _categoriesRepository = categoriesRepository ?? new CategoriesRepository();
            _currenciesRepository = currenciesRepository ?? new CurrenciesRepository();
            _tagsRepository = tagsRepository ?? new TagsRepository();
        }

        public ActionResult Reports()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetReport(int? selectedCurrency, int? selectedMenu, int? selectedSubMenu)
        {
            dynamic report = null;
            var user = CurrentUser;
            var trans = _transRepository.GetTransactions(user.CurrentCompany);
            var cats = _categoriesRepository.GetAll(user.Id);
            var a_currencies = _currenciesRepository.GetCurrencies();
            var tags = _tagsRepository.GetAll(user.Id);

            var currencies =  from t in trans
                          join c in a_currencies on t.CurrencyId equals c.Id
                          where t.Direction == TransactionDirections.Expense
                          group t by c into g
                          select new { Id = g.Key.Id, NameShort = g.Key.NameShort };

            int? currencyId = selectedCurrency ?? (currencies.FirstOrDefault() != null ? currencies.FirstOrDefault().Id : null);

            selectedMenu = selectedMenu ?? 0;
            selectedSubMenu = selectedSubMenu ?? 0;

            if (selectedSubMenu == 0) 
                report = from t in trans 
                         join c in cats on t.CategoryId equals c.Id
                         where t.Direction == TransactionDirections.Expense && t.CurrencyId == currencyId
                         group t.Amount by c into g
                         orderby g.Sum() descending
                         select new { Id = g.Key.Id, Amount = g.Sum(), Name = g.Key.Name }; //

            if (selectedSubMenu == 1)
                report = from t in trans
                         where t.Direction == TransactionDirections.Expense && t.CurrencyId == currencyId
                         group t by t.Description into g
                         orderby g.Sum(t=>t.Amount) descending
                         select new { Id = g.Min(t=>t.Id), Amount = g.Sum(t => t.Amount), Name = g.Key }; //

            if (selectedSubMenu == 2)
                report = from t in trans
                         from tg in tags 
                         where t.Direction == TransactionDirections.Expense && t.CurrencyId == currencyId && t.TagIds.Contains(tg.Id.ToString())
                         group t.Amount by tg into g
                         orderby g.Sum() descending
                         select new { Id = g.Key.Id, Amount = g.Sum(), Name = g.Key.Name }; //


            return Json(new { items = report, currencies, currencyId, selectedMenu, selectedSubMenu });
        }

    }
}
