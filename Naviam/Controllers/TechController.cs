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
        private log4net.ILog logger = log4net.LogManager.GetLogger("navSite");
        private const string stringResult = "ok";

        public void TestConnection()
        { 
        }

        public void UpdateMerchantsCategoriesAsync()
        {
            AsyncManager.OutstandingOperations.Increment();
            Task.Factory.StartNew(() =>
            {
                try
                {
                    CategoriesRepository _categoriesRepository = new CategoriesRepository();
                    _categoriesRepository.GetMerchantsCategories(true);
                }
                catch (Exception ex) 
                { 
                    logger.Error("update categories error!", ex); 
                }
                finally 
                { 
                    AsyncManager.OutstandingOperations.Decrement(); 
                }
            }
            );
        }

        public string UpdateMerchantsCategoriesCompleted(object sender)
        {
            return stringResult;
        }

        //[AsyncTimeout(10000)]
        public void ListDatesAsync(int? daysCount, int? countryId)
        {
            AsyncManager.OutstandingOperations.Increment();
            Task.Factory.StartNew(() =>
                {
                    try 
                    { 
                        AsyncManager.Parameters["result"] = CurrenciesDataAdapter.GetRateAbsentDates(daysCount, countryId); 
                    }
                    catch (Exception ex) 
                    { 
                        logger.Error("get absent dates list async error!", ex); 
                    }
                    finally 
                    { 
                        AsyncManager.OutstandingOperations.Decrement(); 
                    }
                }
            );
        }
        public string ListDatesCompleted(List<string> result)
        {
            string res = Naviam.WebUI.Helpers.SerializationHelper.ToXml(result);
            return res;
        }

        public void UpdateRatesAsync(List<CurrRate> rates)
        {
            AsyncManager.OutstandingOperations.Increment();
            Task.Factory.StartNew(() =>
            {
                try
                {
                    List<Currency> currencies = CurrenciesDataAdapter.GetCurrencies();
                    foreach (CurrRate rate in rates)
                        rate.CurrCode = currencies.FirstOrDefault(x => x.Code.Equals(rate.CurrCode, StringComparison.InvariantCultureIgnoreCase)).Id.ToString();
                    CurrenciesDataAdapter.BulkUpdateRates(rates);
                }
                catch (Exception ex) 
                { 
                    logger.Error("update rates acync error!", ex); 
                }
                finally 
                { 
                    AsyncManager.OutstandingOperations.Decrement(); 
                }
            }
            );
        }

        public string UpdateRatesCompleted()
        {
            return stringResult;
        }

    }


}
