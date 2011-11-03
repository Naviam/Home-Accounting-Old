using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Naviam.Domain.Concrete;

using Naviam.Data;
using System.Globalization;

namespace Naviam.WebUI.Controllers
{
    public class ReportsController : BaseController
    {
        public class GetReportRequest
        {
            public int? selectedCurrency { get; set; }
            public int? selectedMenu { get; set; }
            public int? selectedSubMenu { get; set; }
            public DateTime? startDate { get; set; }
            public DateTime? endDate { get; set; }
        }

        public class ReqMonth
        {
            public int id { get; set; }
            public string name { get; set; }
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

        public string GetMonthName(int month, bool abbreviate, IFormatProvider provider = null)
        {
            DateTimeFormatInfo info = DateTimeFormatInfo.GetInstance(provider);
            if (abbreviate) return info.GetAbbreviatedMonthName(month);
            return info.GetMonthName(month);
        }


        [HttpPost]
        public ActionResult GetReport(GetReportRequest request)
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

            request.selectedCurrency = request.selectedCurrency ?? (currencies.FirstOrDefault() != null ? currencies.FirstOrDefault().Id : null);

            request.selectedMenu = request.selectedMenu ?? 0;
            request.selectedSubMenu = request.selectedSubMenu ?? 0;
            request.startDate = request.startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endYear = DateTime.Now.Year;
            var endMonth = DateTime.Now.Month + 1;
            if (endMonth > 12) { endMonth = 1; endYear++; }
            request.endDate = request.endDate ?? new DateTime(endYear, endMonth, 1);

            var months = new List<ReqMonth>();
            for (int i = 1; i < 13; i++)
            {
                months.Add(new ReqMonth() { id = i, name = GetMonthName(i, false) });
            }

            var headColumn1 = Resources.JavaScript.Category;
            var headColumn2 = Resources.JavaScript.Spending;

            if ((request.selectedMenu == 0 || request.selectedMenu == 1))
            {
                if (request.selectedMenu == 1)
                    headColumn2 = Resources.JavaScript.Income;
                if (request.selectedSubMenu == 0)
                {
                    headColumn1 = Resources.JavaScript.Category;
                    report = from t in trans
                             join c in cats on t.CategoryId equals c.Id
                             where t.Direction == (TransactionDirections)request.selectedMenu && t.CurrencyId == request.selectedCurrency && t.Date >= request.startDate && t.Date < request.endDate
                             group t.Amount by c into g
                             orderby g.Sum() descending
                             select new { Id = g.Key.Id, Amount = g.Sum(), Name = g.Key.Name }; //
                }

                if (request.selectedSubMenu == 1)
                {
                    headColumn1 = Resources.JavaScript.Merchant;
                    report = from t in trans
                             where t.Direction == (TransactionDirections)request.selectedMenu && t.CurrencyId == request.selectedCurrency && t.Date >= request.startDate && t.Date < request.endDate
                             group t by t.Description into g
                             orderby g.Sum(t => t.Amount) descending
                             select new { Id = g.Min(t => t.Id), Amount = g.Sum(t => t.Amount), Name = g.Key }; //
                }

                if (request.selectedSubMenu == 2)
                {
                    headColumn1 = Resources.JavaScript.Tag;
                    report = from t in trans
                             from tg in tags
                             where t.Direction == (TransactionDirections)request.selectedMenu && t.CurrencyId == request.selectedCurrency && t.TagIds.Contains(tg.Id.ToString()) && t.Date >= request.startDate && t.Date < request.endDate
                             group t.Amount by tg into g
                             orderby g.Sum() descending
                             select new { Id = g.Key.Id, Amount = g.Sum(), Name = g.Key.Name }; //
                }
                
                if (request.selectedSubMenu == 3)
                {
                    headColumn1 = Resources.SharedStrings.Month;
                    report = from t in trans
                             join c in months on t.Date.Value.Month equals c.id
                             where t.Direction == (TransactionDirections)request.selectedMenu && t.CurrencyId == request.selectedCurrency && t.Date >= request.startDate && t.Date < request.endDate
                             group t.Amount by c into g
                             orderby g.Key.id
                             select new { Id = g.Key.id, Amount = g.Sum(), Name = g.Key.name }; //
                }
            }

            return Json(new { items = report, currencies, request, headColumn1, headColumn2 });
        }

    }
}
