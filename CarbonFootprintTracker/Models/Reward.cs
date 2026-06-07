namespace CarbonFootprintTracker.Models
{
    public class Reward
    {
        public int Id { get; set; }

        public string Title { get; set; }
        // e.g: "Green Beginner", "Eco Hero"

        public string Description { get; set; }

        public int PointsRequired { get; set; }
        // minimum points needed

        public string BadgeIcon { get; set; }
        // optional image name (stored in wwwroot)
    }
}