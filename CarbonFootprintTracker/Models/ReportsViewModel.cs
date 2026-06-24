namespace CarbonFootprintTracker.Models
{
    public class ReportsViewModel
    {
        public string UserName { get; set; }
        public string UserEmail { get; set; }

        // Weekly Report
        public WeeklyReport WeeklyReport { get; set; }

        // Monthly Report
        public MonthlyReport MonthlyReport { get; set; }

        // Comparison
        public double EmissionChangePercent { get; set; }
        public string TrendMessage { get; set; }
        public string TrendIcon { get; set; }
    }

    public class WeeklyReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double TotalEmission { get; set; }
        public int TotalActivities { get; set; }
        public double AverageDailyEmission { get; set; }
        public string HighestCategory { get; set; }
        public double HighestCategoryValue { get; set; }
        public List<DailyEmission> DailyEmissions { get; set; }
    }

    public class MonthlyReport
    {
        public int Year { get; set; }
        public string MonthName { get; set; }
        public double TotalEmission { get; set; }
        public int TotalActivities { get; set; }
        public double AverageDailyEmission { get; set; }
        public string HighestCategory { get; set; }
        public double HighestCategoryValue { get; set; }
        public List<WeeklyEmission> WeeklyEmissions { get; set; }
    }

    public class DailyEmission
    {
        public DateTime Date { get; set; }
        public double Emission { get; set; }
        public string DayName { get; set; }
    }

    public class WeeklyEmission
    {
        public int WeekNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Emission { get; set; }
    }
}