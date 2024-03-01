using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.Recommend
{
    internal static class Recommendation
    {
        const double HighRiskFraction = 0.08; // 0.16 - 0.3 great, 0.35 - 0.4 mediocre
        internal static double FindRecommendedPosition(double maxRisk, double maxSale, double currentPosition, double riskToleranceAmount, double riskRatio, double numAssets)
        {
            double riskAdjustmentFactor = HighRiskFraction + ((1 - HighRiskFraction) - riskRatio * (1 - HighRiskFraction)); // Invest a fraction in this asset if risk is 100%
            riskAdjustmentFactor = Math.Sqrt(riskAdjustmentFactor); // New to build #26 - To fix risk becoming concentrated in one asset if this factor is too low
            //Min value of above factor now 0.28, Max value 1.0, Mid value 0.48
            double recommendedPosition = (riskToleranceAmount / Math.Max(maxRisk, riskRatio)) * (1 / numAssets) * riskAdjustmentFactor;
            if (recommendedPosition > currentPosition)
            {
                recommendedPosition = currentPosition * (1 - Constants.BuyRatio) + recommendedPosition * Constants.BuyRatio;
            }
            return Math.Max(currentPosition * (1.0 - maxSale), recommendedPosition);
        }
    }
}
