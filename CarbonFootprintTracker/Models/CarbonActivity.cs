using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarbonFootprintTracker.Data;

namespace CarbonFootprintTracker.Models
{
    public class CarbonActivity
    {
        public int Id { get; set; }

        [Required]
        public string ActivityType { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        public string Unit { get; set; }

        public double CarbonEmission { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public string UserId { get; set; }

        // Navigation property - links to the user who created this activity
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}