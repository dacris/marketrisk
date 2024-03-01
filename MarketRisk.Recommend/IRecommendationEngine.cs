using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.Recommend
{
    public interface IRecommendationEngine
    {
        double MaxSale { get; set; }
        double CalculateRiskRatio(double m2, double price, double averageRisk, double averageM2toPriceRatio, double amplitudeM2toPrice, double income);
        double FindRecommendedPosition(double currentPosition, double riskToleranceAmount, double riskRatio, double numAssets);
    }
}
