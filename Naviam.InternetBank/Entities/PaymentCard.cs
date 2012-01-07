using System;

namespace Naviam.InternetBank.Entities
{
    public class PaymentCard
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Currency { get; set; }
        public decimal Balance { get; set; }
        public DateTime RegisterDate { get; set; }
        public DateTime CancelDate { get; set; }
        public string Status { get; set; }
    }
}
