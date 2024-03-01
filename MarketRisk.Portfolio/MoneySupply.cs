using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.Portfolio
{
    public static class MoneySupply
    {
        public const double InitialM2 = 60.0;
        public const double GrowthRate = 1.065;

        // Year 0 = 1929, 60 billion dollars
        public static double CalculateM2(double year)
        {
            return InitialM2 * Math.Pow(GrowthRate, year);
        }

        // Year 0 = 1929, 60 billion dollars
        public static double CalculateM2AdjustedToAsset(double year, double assetGrowthRate)
        {
            return InitialM2 * Math.Pow(Math.Sqrt(assetGrowthRate * GrowthRate), year);
        }
    }
}
