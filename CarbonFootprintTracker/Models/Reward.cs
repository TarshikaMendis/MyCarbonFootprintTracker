using System.ComponentModel.DataAnnotations;

namespace CarbonFootprintTracker.Models
{
    public class Reward
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string BadgeIcon { get; set; }  // Emoji or icon class

        [Required]
        public int RequiredPoints { get; set; }

        [Required]
        public string Category { get; set; }  // Transport, Electricity, Food, Waste, General

        public double RequiredEmissionReduction { get; set; }  // Optional: for reduction-based badges

        public int RequiredActivities { get; set; }  // Optional: for activity count badges

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}