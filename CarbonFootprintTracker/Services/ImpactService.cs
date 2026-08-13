using System.Text.Json;
using System.Text;

namespace CarbonFootprintTracker.Services
{
    public class ImpactStats
    {
        public int TreesPlanted { get; set; }
        public int WasteRecycled { get; set; }
        public int SchoolsHelped { get; set; }
        public decimal TotalRaised { get; set; }
    }

    public class ImpactService
    {
        private readonly string _dataPath;
        private readonly object _lock = new object();

        public ImpactService(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "impact-stats.json");
            EnsureFileExists();
        }

        private void EnsureFileExists()
        {
            if (!File.Exists(_dataPath))
            {
                var defaultStats = new ImpactStats
                {
                    TreesPlanted = 500,
                    WasteRecycled = 2000,
                    SchoolsHelped = 10,
                    TotalRaised = 12500
                };
                var json = JsonSerializer.Serialize(defaultStats);
                File.WriteAllText(_dataPath, json);
            }
        }

        public ImpactStats GetImpactStats()
        {
            lock (_lock)
            {
                var json = File.ReadAllText(_dataPath);
                return JsonSerializer.Deserialize<ImpactStats>(json) ?? new ImpactStats();
            }
        }

        public void UpdateImpact(decimal amount)
        {
            lock (_lock)
            {
                var stats = GetImpactStats();
                stats.TotalRaised += amount;

                // Calculate impact: $10 = 1 tree, $5 = 1kg recycled, $50 = 1 school
                stats.TreesPlanted += (int)(amount / 10);
                stats.WasteRecycled += (int)(amount / 5);
                stats.SchoolsHelped += (int)(amount / 50);

                var json = JsonSerializer.Serialize(stats);
                File.WriteAllText(_dataPath, json);
            }
        }
    }
}