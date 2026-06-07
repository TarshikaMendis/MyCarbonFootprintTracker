using System.ComponentModel.DataAnnotations;

namespace CarbonFootprintTracker.Models
{
    public class CarbonActivity
    {
        public int Id { get; set; }


        [Required]
        public string ActivityType { get; set; }


        [Required]
        public double Amount { get; set; }


        public string Unit { get; set; }


        public double CarbonEmission { get; set; }


        public DateTime Date { get; set; }
            = DateTime.Now;


        public string UserId { get; set; }
    }
}