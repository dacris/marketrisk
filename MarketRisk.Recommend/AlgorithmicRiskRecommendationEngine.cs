using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.Recommend
{
    public class AlgorithmicRiskRecommendationEngine : IRecommendationEngine
    {
        public double MaxSale { get; set; } = 0.2;
        public const double OvervaluedThreshold = 1.25;

        public double CalculateRiskRatio(double m2, double price, double averageRisk, double averageM2toPriceRatio, double amplitudeM2toPrice, double income)
        {
            double reciprocalRisk = new ReciprocalRiskRecommendationEngine().CalculateRiskRatio(m2, price, averageRisk, averageM2toPriceRatio, amplitudeM2toPrice, income);
            if (m2 / price < averageM2toPriceRatio * OvervaluedThreshold)
                return Math.Min(Constants.MaxCalculatedRiskRatio, Math.Pow(reciprocalRisk, 1.0 / 4.0));
            else
                return reciprocalRisk;
        }

        public double FindRecommendedPosition(double currentPosition, double riskToleranceAmount, double riskRatio, double numAssets)
        {
            return Recommendation.FindRecommendedPosition(Constants.MaxRisk, MaxSale, currentPosition, riskToleranceAmount, riskRatio, numAssets);
        }
    }
}
