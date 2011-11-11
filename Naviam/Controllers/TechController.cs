using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Naviam.Domain.Concrete;
using Naviam.Data;
using System.Threading;

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

        //public void GetRates(DateTime date)
        //{
        //    AsyncManager.OutstandingOperations.Increment();
        //    ExRatesSoapClient client = new ExRatesSoapClient();
        //    client.ExRatesDailyCompleted += new EventHandler<ExRatesDailyCompletedEventArgs>(ExRatesDailyCompleted);
        //    client.ExRatesDailyAsync(date);
        //}

        //private void ExRatesDailyCompleted(object sender, ExRatesDailyCompletedEventArgs e)
        //{
        //    AsyncManager.OutstandingOperations.Decrement();
        //}

    }
}
