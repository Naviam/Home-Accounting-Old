using System;

namespace Naviam.InternetBank.Entities
{
    public class ReportPeriod
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Id { get; set; }
        /// <summary>
        /// Tells that report exists in internet bank and can be reused
        /// </summary>
        public bool Exists { get; set; }

        public override string ToString()
        {
            return String.Format("Period {0} from {1} to {2}",
                   Id, StartDate.Date.ToShortDateString(), EndDate.Date.ToShortDateString());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ReportPeriod)) return false;
            return Equals((ReportPeriod) obj);
        }

        public bool Equals(ReportPeriod other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.StartDate.Equals(StartDate) && other.EndDate.Equals(EndDate) && other.CreatedDate.Equals(CreatedDate) && Equals(other.Id, Id) && other.Exists.Equals(Exists);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = StartDate.GetHashCode();
                result = (result*397) ^ EndDate.GetHashCode();
                result = (result*397) ^ CreatedDate.GetHashCode();
                result = (result*397) ^ (Id != null ? Id.GetHashCode() : 0);
                result = (result*397) ^ Exists.GetHashCode();
                return result;
            }
        }
    }
}