
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WakalaPlus.Models
{
    public class Service
    {
        [Key]

        [Column("SERVICE_CODE")]
        public string serviceCode { get; set; }

        [Column("SERVICE_TYPE")]
        public string serviceType { get; set; }

        [Column("SERVICE_NAME")]
        public string serviceName{ get; set; }

        [Column("SERVICE_DESCRIPTION")]
        public string serviceDescription{ get; set; }

        
       

    }
}
