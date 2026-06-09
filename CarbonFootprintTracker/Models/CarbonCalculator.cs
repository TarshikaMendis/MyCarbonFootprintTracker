namespace CarbonFootprintTracker.Models
{
    public static class CarbonCalculator
    {
        public static double Calculate(string activityType, double amount, string unit)
        {
            // Emission factors (kg CO2 per unit)
            switch (activityType)
            {
                case "Transport":
                    if (unit.ToLower() == "km")
                        return Math.Round(amount * 0.21, 2);
                    else if (unit.ToLower() == "mile")
                        return Math.Round(amount * 0.34, 2);
                    else if (unit.ToLower() == "minute")
                        return Math.Round(amount * 0.05, 2);
                    break;

                case "Electricity":
                    if (unit.ToLower() == "kwh" || unit.ToLower() == "unit")
                        return Math.Round(amount * 0.45, 2);
                    break;

                case "Food":
                    if (unit.ToLower() == "meal")
                        return Math.Round(amount * 2.5, 2);
                    else if (unit.ToLower() == "kg")
                        return Math.Round(amount * 5.0, 2);
                    break;

                case "Waste":
                    if (unit.ToLower() == "kg")
                        return Math.Round(amount * 1.2, 2);
                    else if (unit.ToLower() == "bag")
                        return Math.Round(amount * 3.0, 2);
                    break;
            }

            return 0;
        }
    }
}