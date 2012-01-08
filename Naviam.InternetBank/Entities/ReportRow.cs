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
            return String.Format("Period {0} from {1} to {2}",
                   Id, StartDate.Date.ToShortDateString(), EndDate.Date.ToShortDateString());
        }
    }
}