using System;

namespace Naviam.InternetBank.Entities
{
    public class ReportPeriod
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Id { get; set; }
        public bool IsCreated { get; set; }

        public override string ToString()
        {
            return String.Format("Range id {3} with period {0} - {1} is {2}",
                                 StartDate.Date.ToShortDateString(), EndDate.Date.ToShortDateString(), IsCreated ? "created" : "not created", Id);
        }
    }
}