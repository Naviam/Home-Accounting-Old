using System;

namespace Naviam.InternetBank.Entities
{
    public class ReportRow
    {
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Id { get; set; }
        public bool IsCreated { get; set; }

        public override string ToString()
        {
            return String.Format("Range id {3} with period {0} - {1} is {2}",
                                 PeriodStartDate.Date.ToShortDateString(), PeriodEndDate.Date.ToShortDateString(), IsCreated ? "created" : "not created", Id);
        }
    }
}