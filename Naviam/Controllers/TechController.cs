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

        public void ListAsync(int daysCount, int countryId)
        {
            AsyncManager.OutstandingOperations.Increment();
            Task.Factory.StartNew(() => GetRateAbsentDatesData(daysCount, countryId));
        }

        public void GetRateAbsentDatesData(int daysCount, int countryId)
        {
            AsyncManager.Parameters["result"] = CurrenciesDataAdapter.GetRateAbsentDates(daysCount, countryId);
            AsyncManager.OutstandingOperations.Decrement();
        }
        public string ListCompleted(List<DateTime> result)
        {
            string res = Naviam.WebUI.Helpers.SerializationHelper.ToXml(result);
            return res;
        }

        public string UpdateRates2(string rates)
        {

            return "ok";
        }
        public void UpdateRatesAsync(string rates)
        {
           // return "ok";
            AsyncManager.OutstandingOperations.Increment();
            Task.Factory.StartNew(() => UpdateRatesExec(rates));
        }

        public void UpdateRatesExec(string rates)
        {
            List<CurrRate> ratesObj = (List<CurrRate>)Naviam.WebUI.Helpers.SerializationHelper.FromXmlString(typeof(List<CurrRate>), rates);
            CurrenciesDataAdapter.BulkUpdateRates(ratesObj);
            AsyncManager.OutstandingOperations.Decrement();
        }

        public string UpdateRatesCompleted()
        {
            return "";
        }

    }


}
