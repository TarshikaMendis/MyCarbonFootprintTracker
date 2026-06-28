namespace CarbonFootprintTracker.Models
{
    public class AdminDashboardViewModel
    {
        // User Statistics
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }

        // Activity Statistics
        public int TotalActivities { get; set; }
        public int ActivitiesThisMonth { get; set; }

        // Emission Statistics
        public double TotalEmissions { get; set; }
        public double EmissionsThisMonth { get; set; }

        // Reward Statistics
        public int TotalBadgesEarned { get; set; }
        public int TotalRewards { get; set; }

        // Category Breakdown
        public double TransportEmission { get; set; }
        public double ElectricityEmission { get; set; }
        public double FoodEmission { get; set; }
        public double WasteEmission { get; set; }

        // Recent Activities
        public List<RecentActivity> RecentActivities { get; set; }

        // Top Users
        public List<TopUser> TopUsers { get; set; }
    }

    public class RecentActivity
    {
        public string UserName { get; set; }
        public string ActivityType { get; set; }
        public double Amount { get; set; }
        public string Unit { get; set; }
        public double CarbonEmission { get; set; }
        public DateTime Date { get; set; }
    }

    public class TopUser
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public int ActivityCount { get; set; }
        public double TotalEmission { get; set; }
        public int Points { get; set; }
    }

    public class AdminUsersViewModel
    {
        public List<UserInfo> Users { get; set; }
        public int TotalUsers { get; set; }
    }

    public class UserInfo
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int ActivityCount { get; set; }
        public double TotalEmission { get; set; }
        public int Points { get; set; }
        public int BadgesEarned { get; set; }
        public DateTime RegisteredDate { get; set; }
    }

    public class AdminActivitiesViewModel
    {
        public List<AdminActivityInfo> Activities { get; set; }
        public int TotalActivities { get; set; }
        public double TotalEmissions { get; set; }
    }

    public class AdminActivityInfo
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string ActivityType { get; set; }
        public double Amount { get; set; }
        public string Unit { get; set; }
        public double CarbonEmission { get; set; }
        public DateTime Date { get; set; }
    }
}