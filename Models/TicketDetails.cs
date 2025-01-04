
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WakalaPlus.Models
{
    public class TicketDetails
    {
        [Key]

        [Column("TRANSACTION_ID")]
        public string transactionId { get; set; }


        [Column("TICKET_REF")]
        public string ticketRef { get; set; }


        [Column("AGENT_CODE")]
        public string agentCode { get; set; }


        [Column("TICKET_STATUS")]
        public string ticketStatus { get; set; }


        [Column("TICKET_CREATION_DATE_TIME")]
        public DateTime ticketCreationDateTime { get; set; }


        [Column("TICKET_LAST_RESPONSE_DATE_TIME")]
        public DateTime? ticketLastResponseDateTime { get; set; }

    }
}
