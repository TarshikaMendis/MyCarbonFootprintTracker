namespace CarbonFootprintTracker.Models
{
    public class CarbonCalculator
    {
        public static double Calculate(string activityType, double amount, string unit)
        {
            double emission = 0;

            switch (activityType)
            {
                case "Transport":

                    // if km
                    if (unit == "km")
                        emission = amount * 0.21;
                    else if (unit == "mile")
                        emission = amount * 0.13;

                    break;

                case "Electricity":

                    // kWh usage
                    emission = amount * 0.85;
                    break;

                case "Food":

                    emission = amount * 2.5;
                    break;

                case "Waste":

                    emission = amount * 1.8;
                    break;

                default:
                    emission = 0;
                    break;
            }

            return emission;
        }
    }
}