using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.Recommend
{
    public class LinearRiskRecommendationEngine : IRecommendationEngine
    {
        public double MaxSale { get; set; } = 0.2;

        public double CalculateRiskRatio(double m2, double price, double averageRisk, double averageM2toPriceRatio, double amplitudeM2toPrice, double income)
        {
            return Math.Min(Constants.MaxCalculatedRiskRatio, averageRisk / ((m2 / price) / (averageM2toPriceRatio*Constants.MarginOfSafety)));
        }

        public double FindRecommendedPosition(double currentPosition, double riskToleranceAmount, double riskRatio, double numAssets)
        {
            return Recommendation.FindRecommendedPosition(Constants.MaxRisk, MaxSale, currentPosition, riskToleranceAmount, riskRatio, numAssets);
        }
    }
}
