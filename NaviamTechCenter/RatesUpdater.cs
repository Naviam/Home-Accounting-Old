using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NBRBService.NBRBServiceReference;
using System.Data;
using Naviam.DAL;

namespace NaviamTechCenter
{
    class RatesUpdater
    {
        private const int COUNTRY_ID = 1;

        private object lockObject = new object();
        public RatesUpdater()
        {
            ExRatesSoapClient client = null;
            DataSet cursies = null;

            List<DateTime?> dates = CurrenciesDataAdapter.GetRateAbsentDates(100, COUNTRY_ID);
            if (dates != null && dates.Count > 0)
            {
                client = new ExRatesSoapClient();
                foreach (DateTime date in dates)
                {
                    cursies = client.ExRatesDaily(date);
                    if (cursies != null)
                    {

                    }
                }
                //DateTime date = client.LastDailyExRatesDate();
                client.Close();
            }
        }

        // This method is called by the timer delegate.
        public void Update(Object stateInfo)
        {
            ExRatesSoapClient client = null;
            DataSet cursies = null;

            List<DateTime?> dates = CurrenciesDataAdapter.GetRateAbsentDates(100, COUNTRY_ID);
            if (dates != null && dates.Count > 0)
            {
                client = new ExRatesSoapClient();
                foreach (DateTime date in dates)
                {
                    cursies = client.ExRatesDaily(date);
                    if (cursies != null)
                    { 
                    
                    }
                }
                //DateTime date = client.LastDailyExRatesDate();
                client.Close();
            }
            AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
            //Console.WriteLine("{0} Update rate status {1}.", DateTime.Now.ToString("h:mm:ss.fff"), date.ToString());
        }
    }
}
