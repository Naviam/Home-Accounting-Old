using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Naviam.Domain.Concrete;
using Naviam.Data;
using System.Threading;
using Naviam.DAL;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

namespace Naviam.WebUI.Controllers
{
    public class TechController : AsyncController
    {
        private CurrenciesRepository _currenciesRepository = new CurrenciesRepository();
        public void UpdateMerchantsCategories()
        {
            CategoriesRepository _categoriesRepository = new CategoriesRepository();
            AsyncManager.OutstandingOperations.Increment();
            CategoriesRepository.GetMerchantsCategoriesAsynchCaller caller = new CategoriesRepository.GetMerchantsCategoriesAsynchCaller(_categoriesRepository.GetMerchantsCategoriesAsynch);
            IAsyncResult res = caller.BeginInvoke(UpdateMerchantsCategoriesCompleted, null);
        }

        private void UpdateMerchantsCategoriesCompleted(object sender)
        {
            AsyncManager.OutstandingOperations.Decrement();
        }
        [AsyncTimeout(10000)]
        public void ListDatesAsync(int daysCount, int countryId)
        {
            AsyncManager.OutstandingOperations.Increment();
            Task.Factory.StartNew(() =>
                {
                    try { AsyncManager.Parameters["result"] = CurrenciesDataAdapter.GetRateAbsentDates(daysCount, countryId); }
                    catch (Exception ex) { }
                    finally { AsyncManager.OutstandingOperations.Decrement(); }
                }
            );
        }
        public string ListDatesCompleted(List<DateTime> result)
        {
            string res = Naviam.WebUI.Helpers.SerializationHelper.ToXml(result);
            return res;
        }

        public void UpdateRatesAsync(List<CurrRate> rates)
        {
            AsyncManager.OutstandingOperations.Increment();
            //Task.Factory.StartNew(() =>
            //{
            //    try {
            //            List<Currency> currencies = _currenciesRepository.GetCurrencies();
            //            foreach (CurrRate rate in rates)
            //                rate.CurrCode = currencies.FirstOrDefault(x => x.Code.Equals(rate.CurrCode, StringComparison.InvariantCultureIgnoreCase)).Id.ToString();
            //            CurrenciesDataAdapter.BulkUpdateRates(rates);
            //        }
            //    catch (Exception ex) { }
            //    finally { AsyncManager.OutstandingOperations.Decrement(); }
            //}
            //);

            List<Currency> currencies = _currenciesRepository.GetCurrencies();
            foreach (CurrRate rate in rates)
                rate.CurrCode = currencies.FirstOrDefault(x => x.Code.Equals(rate.CurrCode, StringComparison.InvariantCultureIgnoreCase)).Id.ToString();
            CurrenciesDataAdapter.BulkUpdateRates(rates);

            Task.Factory.StartNew(() => UpdateRatesExec(rates));
        }

        public void UpdateRatesExec(List<CurrRate> rates)
        {

            AsyncManager.OutstandingOperations.Decrement();
        }

        public string UpdateRatesCompleted()
        {
            return "ok";
        }

    }


}
