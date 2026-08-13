using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarbonFootprintTracker.Data;

namespace CarbonFootprintTracker.Models
{
    public class Donation
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        public string DonorName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string DonorEmail { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(1, 100000, ErrorMessage = "Amount must be between $1 and $100,000")]
        [Display(Name = "Donation Amount ($)")]
        public decimal Amount { get; set; }

        // ✅ MAKE THESE OPTIONAL (NOT REQUIRED)
        public string? CardNumber { get; set; }
        public string? CardExpiry { get; set; }
        public string? CardCVV { get; set; }
        public string? TransactionId { get; set; }
        public string? Message { get; set; }

        public DateTime DonationDate { get; set; } = DateTime.Now;

        public string? Status { get; set; } = "Completed";

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}