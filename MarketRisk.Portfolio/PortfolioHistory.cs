using MarketRisk.Recommend;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.Portfolio
{
	public class PortfolioHistory
	{
        public class ReadOnlyIndexedProperty<TIndex, TValue>
        {
            readonly Func<TIndex, TValue> GetFunc;

            public ReadOnlyIndexedProperty(Func<TIndex, TValue> getFunc)
            {
                this.GetFunc = getFunc;
            }

            public TValue this[TIndex i]
            {
                get
                {
                    return GetFunc(i);
                }
            }
        }

        private const double MinAmplitude = 2.5;
		private const double MinAmplitudeThreshold = 0.1;
		private int TotalYears { get; set; }
		public Dictionary<string, double> AssetAverageRisks;
		public List<string> AllowedAssets { get; set; }
		public List<Portfolio> AllocationHistory { get; set; }
		public Dictionary<string, List<double>> AssetPriceHistory { get; set; } // map asset type to price history
		public Dictionary<string, double> AssetIncomeRate; // for instance stock dividends
		public Dictionary<string, double> AverageM2toPriceRatios { get; set; } // map asset type to average ratio
		public Dictionary<string, double> M2toPriceAmplitude { get; set; }
        public Dictionary<string, double> AssetSupplyIncrease { get; set; }
            = new Dictionary<string, double>() //TODO: Put this data in a config file
            {
                { "CABond", 0.01 },
                { "USBond", 0.01 },
                //Mining Yield
                { "Gold", 0.0075 },
                { "Silver", 0.01 },
                { "Platinum", 0.025 },
                //Buybacks
                { "SP500", -0.007 },
                { "Dow", -0.01 },
                { "TSX", 0.0075 }
            };

        public ReadOnlyIndexedProperty<string, double> LTRR
        {
            get
            {
                return new ReadOnlyIndexedProperty<string, double>((asset) => Math.Pow(AssetPriceHistory[asset][AssetPriceHistory[asset].Count - 1] / AssetPriceHistory[asset][0], 1.0 / (AssetPriceHistory[asset].Count - 1.0)) - AssetSupplyIncrease.FirstOrDefault(k => k.Key == asset).Value);
            }
        }

        public PortfolioStats Stats { get; set; }

		public void CalculateStats()
		{
			// Call this after allocation history is calculated
			Stats = new PortfolioStats();
			Stats.AnnualReturn = new List<double>();
			Stats.AnnualReturn.Add(1.0);
			for (int year = 1; year < TotalYears; year++)
            {
				double totalInvestedPrevYear = AllocationHistory[year - 1].Assets.Sum(a => a.AmountInvested);
				double annualReturn = 0;
				foreach (string asset in AllowedAssets)
                {
					double assetReturn = AssetIncomeRate[asset] * AssetPriceHistory[asset][year] / AssetPriceHistory[asset][year - 1];
					double fractionInvested = AllocationHistory[year - 1].Assets.Where(a => a.Type == asset).First().AmountInvested / totalInvestedPrevYear;
					annualReturn += fractionInvested * assetReturn;
                }
				Stats.AnnualReturn.Add(annualReturn);
            }
			double lowestReturn = Stats.AnnualReturn.GetRange(TotalYears - 50, 50).Min();
            List<double> rollingTotalReturns = new List<double>();
            // 50 Year Rolling Periods Starting in 1949
            for (int year = 20; year <= TotalYears - 50; year++)
            {
                double totalReturn = Stats.AnnualReturn.GetRange(year, 50).Aggregate((a, b) => a * b);
                rollingTotalReturns.Add(Math.Pow(totalReturn, 1.0/50.0) - 1.0);
            }
            double total50YearsReturn = Stats.AnnualReturn.GetRange(TotalYears - 50, 50).Aggregate((a, b) => a * b);
            Stats.Last50Yr_ChangeInPrice = total50YearsReturn;
			Stats.Last50Yr_LowestReturn = lowestReturn;
            double average50YearsReturn = Math.Pow(total50YearsReturn, 1.0/50.0) - 1.0;
            Stats.Average_50Yr_AnnualReturnVariance = Math.Abs(rollingTotalReturns.Average(r => Math.Abs(r - average50YearsReturn) / 2.0)) * 100;
			Stats.MaxAmountRisked = AllocationHistory.Max((p) => p.Assets.ToArray().Sum(a => a.AmountRisked));
		}
		public void LoadAssetPriceHistory(string assetType, string filePath)
        {
            if (AssetPriceHistory == null)
            {
                AssetPriceHistory = new Dictionary<string, List<double>>();
                AverageM2toPriceRatios = new Dictionary<string, double>();
                AssetAverageRisks = new Dictionary<string, double>();
                M2toPriceAmplitude = new Dictionary<string, double>();
            }
            AssetPriceHistory[assetType] = File.ReadAllLines(filePath).Select(l => double.Parse(l)).ToList();
            // Calculate average M2 to price ratio for the asset
            double year = 0;
            List<double> ratios = new List<double>();
            double ltrr = LTRR[assetType];
            foreach (double price in AssetPriceHistory[assetType])
            {
                double m2 = MoneySupply.CalculateM2AdjustedToAsset(year, ltrr);
                ratios.Add(m2 / price);
                year++;
            }
            AverageM2toPriceRatios[assetType] = GeometricMean(ratios);
            double actualAmplitude = Math.Sqrt(
                    (ratios.Max() / ((ratios.Max() + ratios.Min()) / 2)) * (((ratios.Max() + ratios.Min()) / 2) / ratios.Min())
            );
            // Prefer MinAmplitude except when there is a > +10% deviation, such as is the case with silver
            M2toPriceAmplitude[assetType] = Math.Max(MinAmplitude,
                    actualAmplitude > MinAmplitude + MinAmplitudeThreshold ? actualAmplitude : MinAmplitude
            );
            if (TotalYears == default(int))
            {
                TotalYears = (int)year;
            }
            // Calculate average risk for the asset
            // "Risk" is any return that is lower than our expected return
            double expectedRealReturn = 0.09;
            double expectedReturn = MoneySupply.GrowthRate + expectedRealReturn;
            expectedReturn += MoneySupply.GrowthRate - ltrr;
            List<double> subparReturns = new List<double>();
            subparReturns.AddRange(AssetPriceHistory[assetType].Select(
                (p, index) => index == 0 ? 1.0 : AssetPriceHistory[assetType][index] / AssetPriceHistory[assetType][index - 1])
                .Where(d => d < expectedReturn));
            if (subparReturns.Count == 0)
            {
                subparReturns.Add(0.95); // default value
            }
            double averageRisk = Math.Min(0.975, expectedReturn - subparReturns.Average() + MoneySupply.GrowthRate - 1);
            AssetAverageRisks[assetType] = averageRisk;
        }

        private static double GeometricMean(List<double> ratios)
        {
            return Math.Pow(ratios.Aggregate((a, b) => a * b), 1.0 / ratios.Count);
        }

        public void CalculateAllocationHistory(IRecommendationEngine rec, double initialRiskToleranceAmount, double inflationRate, double realRiskGrowthRate)
		{
			AllocationHistory = new List<Portfolio>();
			// Call this after loading asset prices and setting AllowedAssets and AssetAverageRisks
			const int modYrs = 1;
			for (int year = 0; year < TotalYears; year++)
            {
				Portfolio p = new Portfolio { Assets = new List<Asset>() };
				Portfolio previousYear = year > 0 ? AllocationHistory[year - 1] : null;
				double riskFinalAmount = initialRiskToleranceAmount * Math.Pow(inflationRate + realRiskGrowthRate, year);
				if (year % modYrs == 0)
				{
					foreach (string assetType in AllowedAssets)
					{
						double ltrr = LTRR[assetType];
						double riskRatio = rec.CalculateRiskRatio(
							MoneySupply.CalculateM2AdjustedToAsset(year, ltrr),
							AssetPriceHistory[assetType][year],
							AssetAverageRisks[assetType],
							AverageM2toPriceRatios[assetType],
							M2toPriceAmplitude[assetType],
                            AssetIncomeRate[assetType]);
						double amount = rec.FindRecommendedPosition(
							previousYear != null ? previousYear.Assets.Where(a => a.Type == assetType).First().AmountInvested : 0,
							riskFinalAmount,
							riskRatio,
							AllowedAssets.Count
							);
						p.Assets.Add(new Asset { Type = assetType, AmountInvested = amount, AmountRisked = riskRatio * amount });
					}
				}
                else
                {
					p.Assets = new List<Asset>(previousYear.Assets);
                }
				p.BalanceAllocationsForMaxRisk(riskFinalAmount);
				AllocationHistory.Add(p);
            }
		}
	}
}
