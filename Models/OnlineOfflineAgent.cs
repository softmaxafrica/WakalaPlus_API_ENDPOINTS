 using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WakalaPlus.Models

    {

       public class OnlineOfflineAgent
        {

        [Key]

        [Column("TRANSACTION_ID")]
        public string transactionId { get; set; }


        [Column("AGENT_CODE")]
        public string? agentCode { get; set; }
      
        [Column("AGENT_FULL_NAME")]
        public string? agentFullName { get; set; }

        [Column("AGENT_PHONE")]
          public string? agentPhone { get; set; }

        [Column("ADDRESS")]
        public string? address { get; set; }

        [Column("LONGITUDE")]
        public double? longitude { get; set; }

        [Column("LATITUDE")]
        public double? latitude { get; set; }
        [Column("SERVICE_GROUP_CODE")]
        public string? serviceGroupCode { get; set; }

        [Column("NETWORKS_OPERATING")]

        public string? networksOperating { get; set; }

        [Column("STATUS")]
        public string? status { get; set; }

        [Column("LAST_UPDATE")]
        public DateTime? lastUpdate { get; set; }

    }
}
