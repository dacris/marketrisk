using MarketRisk.Recommend.Planning;
using System.Collections.Generic;

namespace MarketRisk.GUI
{
    public class State
    {
        public List<string> AssetCombination { get; set; }
        public Dictionary<string, double> AssetPrices { get; set; }
        public Dictionary<string, double> AssetPositions { get; set; }
        public Dictionary<string, List<double>> BondYields { get; set; }
        public Dictionary<string, double> AssetExchangeRates { get; set; }
        public string RiskAmount { get; set; }
        public PlanInput PlanInput { get; set; }
    }
}