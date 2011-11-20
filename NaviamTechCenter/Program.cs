using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using log4net.Config;
using System.Configuration;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "NaviamTechCenter.exe.config", Watch = true)]

namespace NaviamTechCenter
{

    public class Program
    {
        public static log4net.ILog logger = log4net.LogManager.GetLogger("navTechCenter");

        static void Main(string[] args)
        {
            int dueTime;
            int period;

            //set rates update timer
            RatesUpdater ratesUpdater = new RatesUpdater();
            TimerCallback timerCallback1 = ratesUpdater.Update;
            dueTime = 1000 * 60 * int.Parse(ConfigurationManager.AppSettings["nbrb_rates_start_after"]);
            period = 1000 * 60 * int.Parse(ConfigurationManager.AppSettings["nbrb_rates_update_interval"]);
            Timer ratesUpdateTimer = new Timer(timerCallback1, new AutoResetEvent(false), dueTime, period);

            //set categories info refresh timer
            CategoriesMerchantsUpdater categoriesMerchantsUpdater = new CategoriesMerchantsUpdater();
            TimerCallback timerCallback2 = categoriesMerchantsUpdater.Update;
            dueTime = 1000 * 60 * int.Parse(ConfigurationManager.AppSettings["cat_start_after"]);
            period = 1000 * 60 * int.Parse(ConfigurationManager.AppSettings["cat_update_interval"]);
            Timer categoriesMerchantsUpdateTimer = new Timer(timerCallback2, new AutoResetEvent(false), dueTime, period);

            Console.Read();
        }

    }
}
