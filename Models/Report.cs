
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WakalaPlus.Models
{
    public class Report
    {
        [Key]

        [Column("TRANSACTION_ID")]
        public string transactionId { get; set; }

        [Column("SERVICE_CODE")]
        public string serviceCode { get; set; }

        [Column("AGENT_CODE")]
        public string agentCode { get; set; }

        [Column("COMMENTS")]
        public string comments{ get; set; }

        [Column("CUSTOMER_PHONE_NUMBER")]
        public string customerPhoneNumber{ get; set; }

        
       

    }
}
