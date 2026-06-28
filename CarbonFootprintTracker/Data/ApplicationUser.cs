using Microsoft.AspNetCore.Identity;

namespace CarbonFootprintTracker.Data
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsAdmin { get; set; } = false;
    }
}