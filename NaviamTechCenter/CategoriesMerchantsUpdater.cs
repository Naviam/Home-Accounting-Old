using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NaviamTechCenter
{
    class CategoriesMerchantsUpdater
    {
        
        public CategoriesMerchantsUpdater()
        {
        }

        // This method is called by the timer delegate.
        public void Update(Object stateInfo)
        {
            AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
            Console.WriteLine("{0} Update merchant category id.", DateTime.Now.ToString("h:mm:ss.fff"));
        }
    }
}
