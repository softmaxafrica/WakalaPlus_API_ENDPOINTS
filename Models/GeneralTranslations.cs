
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WakalaPlus.Models
{
    public class GeneralTranslations
    {
        [Key]

        [Column("CODE")]
        public string code { get; set; }

        [Column("VALUE")]
        public string? value{ get; set; }

        [Column("DESCRIPTION")]
        public string description{ get; set; }

        [Column("TABLE_NAME")]
        public string? tableName { get; set; }


    }
}
