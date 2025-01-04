
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Math;
using WakalaPlus.Shared;

namespace WakalaPlus.Models
{
    public class Agent
    {
        [Key]

        [Column("AGENT_CODE")]
        public string agentCode { get; set; }

        [Column("PASSWORD")]
        public string password { get; set; }

        [Column("NIDA")]
        public string nida { get; set; }

        [Column("AGENT_FULL_NAME")]
        public string agentFullName { get; set; }

        [Column("AGENT_PHONE")]
        public string agentPhone { get; set; }

        

        [Column("NETWORKS_OPERATING")]
        public string networksOperating { get; set; }

        [Column("SERVICE_GROUP_CODE")]
        public string serviceGroupCode { get; set; }


        [Column("REGISTRATION_STATUS")]
        public string regstrationStatus { get; set; }


        [Column("ADDRESS")]
        public string? address { get; set; }


        [Column("LONGITUDE")]
        public double? longitude { get; set; }

        [Column("LATITUDE")]
        public double? latitude { get; set; }





    }


}
