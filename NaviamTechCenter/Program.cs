using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using log4net.Config;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "NaviamTechCenter.exe.config", Watch = true)]

namespace NaviamTechCenter
{
    
    public class Program
    {
        public static log4net.ILog logger = log4net.LogManager.GetLogger("navTechCenter");

        static void Main(string[] args)
        {
            //set rates update timer
            RatesUpdater ratesUpdater = new RatesUpdater();
            TimerCallback timerCallback1 = ratesUpdater.Update;
            Timer ratesUpdateTimer = new Timer(timerCallback1, new AutoResetEvent(false), 0, 1000000);

            Console.Read();

            return;
            CategoriesMerchantsUpdater categoriesMerchantsUpdater = new CategoriesMerchantsUpdater();
            TimerCallback timerCallback2 = categoriesMerchantsUpdater.Update;
            Timer categoriesMerchantsUpdateTimer = new Timer(timerCallback2, new AutoResetEvent(false), 0, 10000);

            Console.Read();
        }

    }
}
