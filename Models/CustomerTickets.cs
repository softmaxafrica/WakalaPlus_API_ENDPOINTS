
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WakalaPlus.Models
{
    public class CustomerTickets
    {
        [Key]

        [Column("TRANSACTION_ID")]
        public string transactionId { get; set; }
        [Column("AGENT_CODE")]
        public string? agentCode { get; set; }

        [Column("PHONE_NUMBER")]
        public  string? phoneNumber { get; set; }

        [Column("DESCRIPTION")]
        public string? description { get; set; }

        [Column("CUSTOMER_LONGITUDE")]
        public double? customerLongitude { get; set; }

        [Column("CUSTOMER_LATITUDE")]
        public double? customerLatitude { get; set; }

        [Column("AGENT_LONGITUDE")]
        public double? agentLongitude { get; set; }
        [Column("AGENT_LATITUDE")]
        public double? agentLatitude { get; set; }

        [Column("SERVICE_REQUESTED")]
        public string? serviceRequested { get; set; }

        [Column("NETWORK")]
        public string? network { get; set; }

        [Column("TICKET_STATUS")]
        public string ticketStatus { get; set; }

        [Column("TICKET_CREATION_DATE_TIME")]
        public DateTime ticketCreationDateTime { get; set; }

        [Column("TICKET_LAST_RESPONSE_DATE_TIME")]
        public DateTime? ticketLastResponseDateTime { get; set; }



 

   
 
    }
}
