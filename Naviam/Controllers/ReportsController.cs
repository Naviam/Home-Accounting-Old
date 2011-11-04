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
            public int? selectedTimeFrame { get; set; }
            public int? selectedTimeFrameStart { get; set; }
            public int? selectedTimeFrameEnd { get; set; }
            public DateTime? startDate { get; set; }
            public DateTime? endDate { get; set; }
        }

        public class ReqMonth
        {
            public int id { get; set; }
            public int year { get; set; }
            public string name { get; set; }
            public string shortName { get; set; }
        }

        public enum ReportType
        {
            ByExpense,
            ByIncome
        }

        public enum ReportSubtype
        {
            ByCategory,
            ByMerchant,
            ByTag,
            OverTime
        }
        
        public enum ReportTimeFrame
        {
            ThisMonth,
            LastMonth,
            ThisYear,
            AllTime,
            Custom
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

            request.selectedMenu = request.selectedMenu ?? (int)ReportType.ByExpense;
            request.selectedSubMenu = request.selectedSubMenu ?? (int)ReportSubtype.ByCategory;
            request.selectedTimeFrame = request.selectedTimeFrame ?? (int)ReportTimeFrame.ThisMonth;

            string timeFrameDesc = "";

            var firstTrans = trans.OrderBy(t => t.Date).FirstOrDefault();
            var firstTransDate = firstTrans != null ? firstTrans.Date : null;
            var lastTrans = trans.OrderByDescending(t => t.Date).FirstOrDefault();
            var lastTransDate = lastTrans != null ? lastTrans.Date : null;

            if (request.selectedTimeFrame == (int)ReportTimeFrame.ThisMonth)
            {
                request.startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                request.endDate = request.startDate.Value.AddMonths(1);
            }
            if (request.selectedTimeFrame == (int)ReportTimeFrame.LastMonth)
            {
                request.startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
                request.endDate = request.startDate.Value.AddMonths(1);
            }
            if (request.selectedTimeFrame == (int)ReportTimeFrame.ThisYear)
            {
                request.startDate = new DateTime(DateTime.Now.Year, 1, 1);
                if (request.startDate < firstTransDate)
                    request.startDate = firstTransDate;
                request.endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
            }
            if (request.selectedTimeFrame == (int)ReportTimeFrame.AllTime)
            {
                request.startDate = firstTransDate;
                request.endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
            }
            if (request.selectedTimeFrame == (int)ReportTimeFrame.Custom)
            {
                int month = request.selectedTimeFrameStart.Value - (request.selectedTimeFrameStart.Value / 100) * 100;
                request.startDate = new DateTime(request.selectedTimeFrameStart.Value / 100, month, 1);
                month = request.selectedTimeFrameEnd.Value - (request.selectedTimeFrameEnd.Value / 100) * 100;
                request.endDate = new DateTime(request.selectedTimeFrameEnd.Value / 100, month, 1).AddMonths(1);
            }
            request.selectedTimeFrameStart = request.startDate.Value.ToYearMonth();
            request.selectedTimeFrameEnd = request.endDate.Value.AddMonths(-1).ToYearMonth();
            timeFrameDesc = String.Format("{0} {1}", GetMonthName(request.startDate.Value.Month, false), request.startDate.Value.Year);
            DateTime endFrame = request.endDate.Value.AddMonths(-1);
            if (request.startDate.Value.Month != endFrame.Month)
                timeFrameDesc += String.Format(" - {0} {1}", GetMonthName(endFrame.Month, false), endFrame.Year);

            //request.startDate = new DateTime(2010, 1, 1);
            //request.endDate = new DateTime(DateTime.Now.Year, 1, 1).AddYears(1);
            var months = new List<ReqMonth>();
            for (int i = request.startDate.Value.ToYearMonth(); i < request.endDate.Value.ToYearMonth(); i++)
            {
                int month = i - (i / 100) * 100;
                if (month == 13)
                {
                    month = 1;
                    i += 88;
                }
                months.Add(new ReqMonth() { id = i, year = i / 100, name = GetMonthName(month, false), shortName = GetMonthName(month, true) });
            }
            
            //period of all transactions
            var transTimeFrame = new List<ReqMonth>();
            for (int i = firstTransDate.Value.ToYearMonth(); i <= lastTransDate.Value.ToYearMonth(); i++)
            {
                int month = i - (i / 100) * 100;
                if (month == 13)
                {
                    month = 1;
                    i += 88;
                }
                transTimeFrame.Add(new ReqMonth() { id = i, year = i / 100, name = GetMonthName(month, false), shortName = GetMonthName(month, true) });
            }

            var headColumn1 = Resources.JavaScript.Category;
            var headColumn2 = Resources.JavaScript.Spending;

            if ((request.selectedMenu == (int)ReportType.ByExpense || request.selectedMenu == (int)ReportType.ByIncome))
            {
                if (request.selectedMenu == (int)ReportType.ByIncome)
                    headColumn2 = Resources.JavaScript.Income;
                if (request.selectedSubMenu == (int)ReportSubtype.ByCategory) 
                {
                    headColumn1 = Resources.JavaScript.Category;
                    report = from t in trans
                             join c in cats on t.CategoryId equals c.Id
                             where t.Direction == (TransactionDirections)request.selectedMenu && t.CurrencyId == request.selectedCurrency && t.Date >= request.startDate && t.Date < request.endDate
                             group t.Amount by c into g
                             orderby g.Sum() descending
                             select new { Id = g.Key.Id, Amount = g.Sum(), Name = g.Key.Name }; //
                }

                if (request.selectedSubMenu == (int)ReportSubtype.ByMerchant)
                {
                    headColumn1 = Resources.JavaScript.Merchant;
                    report = from t in trans
                             where t.Direction == (TransactionDirections)request.selectedMenu && t.CurrencyId == request.selectedCurrency && t.Date >= request.startDate && t.Date < request.endDate
                             group t by t.Description into g
                             orderby g.Sum(t => t.Amount) descending
                             select new { Id = g.Min(t => t.Id), Amount = g.Sum(t => t.Amount), Name = g.Key }; //
                }

                if (request.selectedSubMenu == (int)ReportSubtype.ByTag) 
                {
                    headColumn1 = Resources.JavaScript.Tag;
                    report = from t in trans
                             from tg in tags
                             where t.Direction == (TransactionDirections)request.selectedMenu && t.CurrencyId == request.selectedCurrency && t.TagIds.Contains(tg.Id.ToString()) && t.Date >= request.startDate && t.Date < request.endDate
                             group t.Amount by tg into g
                             orderby g.Sum() descending
                             select new { Id = g.Key.Id, Amount = g.Sum(), Name = g.Key.Name }; //
                }

                if (request.selectedSubMenu == (int)ReportSubtype.OverTime) 
                {
                    headColumn1 = Resources.SharedStrings.Month;
                    report = from t in trans
                             join c in months on t.Date.Value.ToYearMonth() equals c.id
                             where t.Direction == (TransactionDirections)request.selectedMenu && t.CurrencyId == request.selectedCurrency && t.Date >= request.startDate && t.Date < request.endDate
                             group t.Amount by c into g
                             orderby g.Key.id
                             select new { Id = g.Key.id, Amount = g.Sum(), Name = g.Key.name }; //
                }
            }

            return Json(new { items = report, currencies, request, headColumn1, headColumn2, timeFrameDesc, transTimeFrame });
        }

    }
}
