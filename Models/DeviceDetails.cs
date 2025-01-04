using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WakalaPlus.Models
{
    public class DeviceDetails
    {
        [Key]
        [Column("DEVICE_ID")]
        public string deviceId { get; set; }
        [Column("IDENTITY")]
        public string Identity { get; set; }
        [Column("LAST_CONNECTION_DATE")]
        public DateTime? LastConnectionDate { get; set; }

        [Column("CREATED_DATE")]
        public DateTime? createdDate { get; set; }
        [Column("CONNECTION_ID")]
        public string connectionId { get; set; }

        [Column("LAST_ACTION")]
        public string LastAction { get; set; }
        
    }
}
