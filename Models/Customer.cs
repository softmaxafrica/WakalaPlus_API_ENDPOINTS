using System.ComponentModel.DataAnnotations.Schema;

namespace WakalaPlus.Models
{
    public class Customer
    {
        public string? customerId { get; set; }
        public string? FullName { get; set; }
        public string? Nida { get; set; }
        public double? longitude { get; set; }
        public double? latitude { get; set; }
        public DateTime? RegDate{ get; set; }

    }
}
