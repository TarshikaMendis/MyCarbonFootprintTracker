using System.ComponentModel.DataAnnotations;

namespace CarbonFootprintTracker.Models
{
    public class CarbonRecord
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public double TransportEmission { get; set; }

        public double ElectricityEmission { get; set; }

        public double FoodEmission { get; set; }

        public double WasteEmission { get; set; }

        public double TotalEmission { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
    }
}