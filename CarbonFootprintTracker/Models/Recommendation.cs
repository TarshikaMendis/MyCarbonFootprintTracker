namespace CarbonFootprintTracker.Models
{
    public class Recommendation
    {
        public int Id { get; set; }

        public string Category { get; set; }
        // e.g: Transport, Electricity, Food

        public string Message { get; set; }
        // e.g: "Use public transport twice a week"

        public double MinEmission { get; set; }
        // trigger condition

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}