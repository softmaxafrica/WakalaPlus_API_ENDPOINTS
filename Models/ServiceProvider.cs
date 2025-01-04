using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WakalaPlus.Models
{
    public class ServiceProviders
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("NAME")]
        public string? Name { get; set; }

        [Column("IMAGE_URL")]
        public string? ImageUrl { get; set; }

        [Column("CATEGORY")]
        public string? Category { get; set; } // E.g., "Mitandao", "Bank"
    }
}
