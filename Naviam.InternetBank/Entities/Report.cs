using System;
using System.Collections.Generic;

namespace Naviam.InternetBank.Entities
{
    public class Report
    {
        public string CardNumber { get; set; }
        public DateTime GeneratedDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; }
        public decimal BlockedAmount { get; set; }
        public decimal StartBalance { get; set; }
        public IEnumerable<AccountTransaction> Transactions { get; set; }
    }
}