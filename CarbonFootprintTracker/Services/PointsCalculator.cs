using CarbonFootprintTracker.Models;

namespace CarbonFootprintTracker.Services
{
    public static class PointsCalculator
    {
        public static int CalculatePoints(string activityType, double amount, double carbonEmission)
        {
            int points = 0;

            // Base points for adding an activity
            points += 10;

            // Points based on activity type and carbon emission (lower emission = more points)
            switch (activityType)
            {
                case "Transport":
                    if (carbonEmission < 1)
                        points += 20;  // Walking, cycling
                    else if (carbonEmission < 5)
                        points += 10;  // Public transport, carpool
                    else
                        points += 5;   // Personal car
                    break;

                case "Electricity":
                    if (carbonEmission < 5)
                        points += 15;  // Energy efficient
                    else if (carbonEmission < 15)
                        points += 8;
                    else
                        points += 3;
                    break;

                case "Food":
                    if (carbonEmission < 3)
                        points += 15;  // Plant-based meals
                    else if (carbonEmission < 8)
                        points += 8;
                    else
                        points += 3;   // High meat consumption
                    break;

                case "Waste":
                    if (carbonEmission < 2)
                        points += 15;  // Minimal waste, recycling
                    else if (carbonEmission < 6)
                        points += 8;
                    else
                        points += 3;
                    break;

                default:
                    points += 5;
                    break;
            }

            // Bonus points for logging consistently
            return points;
        }

        // Calculate total points for a user based on all activities
        public static int CalculateTotalUserPoints(List<CarbonActivity> activities)
        {
            int totalPoints = 0;
            foreach (var activity in activities)
            {
                totalPoints += CalculatePoints(activity.ActivityType, activity.Amount, activity.CarbonEmission);
            }
            return totalPoints;
        }
    }
}