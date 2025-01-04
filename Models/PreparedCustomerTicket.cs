namespace WakalaPlus.Models
{
    public class PreparedCustomerTicket
    {
        public string transactionId { get; set; }
        public string? phoneNumber { get; set; }
        public string? description { get; set; }
        public string network { get; set; }
        public string serviceRequested { get; set; }
        public double custLatitude { get; set; }
        public double custLongitude { get; set; }
        public string agentCode { get; set; } 
        public double? agentLongitude { get; set; }
        public double? agentLatitude { get; set; }
        public DateTime? createdDate { get; set; }
        public DateTime? LastResponseDateTime { get; set; }

    }
}
