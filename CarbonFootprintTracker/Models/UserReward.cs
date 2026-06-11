using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarbonFootprintTracker.Data;

namespace CarbonFootprintTracker.Models
{
    public class UserReward
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int RewardId { get; set; }

        public int PointsEarned { get; set; }

        public DateTime EarnedAt { get; set; } = DateTime.Now;

        public bool IsViewed { get; set; } = false;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("RewardId")]
        public virtual Reward Reward { get; set; }
    }
}