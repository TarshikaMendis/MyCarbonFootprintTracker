namespace CarbonFootprintTracker.Models
{
    public class DashboardViewModel
    {
        // User Info
        public string UserName { get; set; }
        public string UserEmail { get; set; }

        // Total Emissions
        public double TotalEmission { get; set; }

        // Category-wise Emissions
        public double TransportEmission { get; set; }
        public double ElectricityEmission { get; set; }
        public double FoodEmission { get; set; }
        public double WasteEmission { get; set; }

        // This Month's Emissions
        public double ThisMonthEmission { get; set; }

        // Highest Emission Source
        public string HighestEmissionSource { get; set; }
        public double HighestEmissionValue { get; set; }

        // AI Recommendation (Rule-based)
        public string AiRecommendation { get; set; }
        public string RecommendationIcon { get; set; }

        // Activity Count
        public int TotalActivities { get; set; }

        //  Chart Data
        public List<ChartData> DailyEmissions { get; set; }
        public List<ChartData> MonthlyEmissions { get; set; }
    }

    //  Chart Data Class
    public class ChartData
    {
        public string Label { get; set; }
        public double Value { get; set; }
    }
}