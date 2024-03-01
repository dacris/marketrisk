using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.Recommend
{
    /// <summary>
    /// This method of estimating risk used to yield the best performance in build 9 & earlier.
    /// 
    /// Cubic actually has slightly better results for stocks but worse for everything else.
    /// 
    /// The drawback is that risk ratio gets saturated (reaches maximum) too easily.
    /// </summary>
    public class QuadraticRiskRecommendationEngine : IRecommendationEngine
    {
        public double MaxSale { get; set; } = 0.2;

        public double CalculateRiskRatio(double m2, double price, double averageRisk, double averageM2toPriceRatio, double amplitudeM2toPrice, double income)
        {
            return Math.Min(Constants.MaxCalculatedRiskRatio, averageRisk / (Math.Pow((m2 / price) / (averageM2toPriceRatio*Constants.MarginOfSafety), 2.0)));
        }

        public double FindRecommendedPosition(double currentPosition, double riskToleranceAmount, double riskRatio, double numAssets)
        {
            return Recommendation.FindRecommendedPosition(Constants.MaxRisk, MaxSale, currentPosition, riskToleranceAmount, riskRatio, numAssets);
        }
    }
}
