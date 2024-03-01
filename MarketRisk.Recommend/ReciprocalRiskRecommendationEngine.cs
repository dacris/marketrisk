using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.Recommend
{
    /// <summary>
    /// When tuned correctly, this engine has better overall performance than quadratic.
    /// </summary>
    public class ReciprocalRiskRecommendationEngine : IRecommendationEngine
    {
        public double MaxSale { get; set; } = 0.2;
        public const double AmplitudeFactor = 0.5;

        public double CalculateRiskRatio(double m2, double price, double averageRisk, double averageM2toPriceRatio, double amplitudeM2toPrice, double income)
        {
            double typicalHoldYears = 3;
            double neutralRateOfReturn = 0.0;
            // Because we use averageRisk in the formula & margin of safety, expected rate of return (9% real) is already factored in.
            // Optimal Calculation
            //return Math.Min(Constants.MaxCalculatedRiskRatio, averageRisk / 4.0 + (1.0 - averageRisk / 4.0) - (1.0 - averageRisk / 4.0) / Math.Max(1.0, ((price * AmplitudeFactor * amplitudeM2toPrice / m2) / (1 / (averageM2toPriceRatio * Constants.MarginOfSafety)))));
            // First-principles Calculation
            double expectedPrice = (m2 / averageM2toPriceRatio) / (amplitudeM2toPrice * AmplitudeFactor);
            double actualPrice = price * Constants.MarginOfSafety / Math.Pow(income, typicalHoldYears);
            double risk = Math.Max(averageRisk / 4.0, averageRisk / 4.0 + (1 - averageRisk / 4.0) - (1.0 - averageRisk / 4.0) * (expectedPrice / (actualPrice * (1.0 + neutralRateOfReturn))));
            return Math.Min(Constants.MaxCalculatedRiskRatio, risk);
        }

        public double FindRecommendedPosition(double currentPosition, double riskToleranceAmount, double riskRatio, double numAssets)
        {
            return Recommendation.FindRecommendedPosition(Constants.MaxRisk, MaxSale, currentPosition, riskToleranceAmount, riskRatio, numAssets);
        }
    }
}
