using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarbonFootprintTracker.Data;

namespace CarbonFootprintTracker.Models
{
    public class Donation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string DonorName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string DonorEmail { get; set; }

        [Required]
        [Range(1, 100000)]
        [Display(Name = "Donation Amount ($)")]
        public decimal Amount { get; set; }

        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }

        [Display(Name = "Expiry Date")]
        public string CardExpiry { get; set; }

        [Display(Name = "CVV")]
        public string CardCVV { get; set; }

        public string TransactionId { get; set; }

        public string Message { get; set; }

        public DateTime DonationDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Completed";

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}