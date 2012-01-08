using System;

namespace Naviam.InternetBank.Entities
{
    public class StateOnDate
    {
        public DateTime Date { get; set; }
        public decimal Available { get; set; }
        public decimal BlockedAmount { get; set; }
        public decimal CreditLimit { get; set; }

        public override string ToString()
        {
            return String.Format("Date: {0}, Available: {1}, BlockedAmount: {2}, CreditLimit: {3}",
                                 Date.Date, Available, BlockedAmount, CreditLimit);
        }
    }
}