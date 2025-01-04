using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WakalaPlus.Models
{
    public class NotificationMessages
    {
        [Key]
        [Column("CONNECTION_ID")]

        public string connectionId { get; set; }
        [Column("SENDER_IDENTITY")]

        public string senderIdentity { get; set; }
        [Column("RECEIVER_CONNECTION_ID")]

        public string receiverConnectionId { get; set; }
        [Column("RECEIVER_IDENTITY")]

        public string receiverIdentity { get; set; }
        [Column("MESSAGE")]

        public string message { get; set; }
        [Column("STATUS")]
        public string status { get; set; }
    }
}
