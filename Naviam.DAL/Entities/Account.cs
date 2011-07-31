using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Naviam.Data
{
    [Serializable]
    public class Account : DbEntity
    {
        public enum AccountTypes { Bank=0, WebMoney, Home }
        const string Bank = "bank";
        const string WebMoney = "webmoney";
        const string Home = "home";

        public static AccountTypes GetAccountType(string accTypeStr)
        {
            switch (accTypeStr)
            {
                case Bank: return AccountTypes.Bank;
                case WebMoney: return AccountTypes.WebMoney;
                case Home: return AccountTypes.Home;
                default: return AccountTypes.Bank;
            }
        }
    }
}
