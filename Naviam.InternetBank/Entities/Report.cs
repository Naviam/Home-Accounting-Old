using System;
using System.Collections.Generic;

namespace Naviam.InternetBank.Entities
{
    public class Report
    {
        public string CardNumber { get; set; }
        public ReportPeriod ReportPeriod { get; set; }
        public StateOnDate StateOnDate { get; set; }
        public string Currency { get; set; }
        public decimal StartBalance { get; set; }
        public IEnumerable<AccountTransaction> Transactions { get; set; }
    }

    public class StateOnDate
    {
        public DateTime Date { get; set; }
        public decimal Available { get; set; }
        public decimal BlockedAmount { get; set; }
        public decimal CreditLimit { get; set; }
    }
}