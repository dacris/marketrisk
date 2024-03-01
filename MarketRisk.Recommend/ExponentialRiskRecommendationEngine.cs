using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.Recommend
{
    public class ExponentialRiskRecommendationEngine : IRecommendationEngine
    {
        public double MaxSale { get; set; } = 0.2;
        public const double AdjustmentFactor = 0.9;

        public double CalculateRiskRatio(double m2, double price, double averageRisk, double averageM2toPriceRatio, double amplitudeM2toPrice, double income)
        {
            return Math.Min(Constants.MaxCalculatedRiskRatio, Math.Max(averageRisk / 8.0, Math.Pow((1/averageRisk), -(m2 / price) / (AdjustmentFactor * averageM2toPriceRatio * Constants.MarginOfSafety))));
        }

        public double FindRecommendedPosition(double currentPosition, double riskToleranceAmount, double riskRatio, double numAssets)
        {
            return Recommendation.FindRecommendedPosition(Constants.MaxRisk, MaxSale, currentPosition, riskToleranceAmount, riskRatio, numAssets);
        }
    }
}
