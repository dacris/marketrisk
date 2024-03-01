using MarketRisk.Portfolio;
using AssetPortfolio = MarketRisk.Portfolio.Portfolio;
using MarketRisk.Recommend;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using MarketRisk.Testing;

namespace MarketRisk.Core.Console
{
    class Program
    {
        private const int StartYear = 1929;

        private static readonly Tester tester = new Tester();
        private static List<string> assetCombination = new List<string>();
        private static Dictionary<string, double> assetPrices = new Dictionary<string, double>();
        private static Dictionary<string, double> assetPositions = new Dictionary<string, double>();
        private static Dictionary<string, double> assetExchangeRates = new Dictionary<string, double>();
        private static Dictionary<string, List<double>> bondYields = new Dictionary<string, List<double>>();
        private static Dictionary<string, AssetConfig> assetConfig = new Dictionary<string, AssetConfig>();
        private static string riskAmount = string.Empty;
        private static AssetPortfolio portfolio;

        private static void CalculateMetrics(string asset, out double amountToInvest, out double amountRisked)
        {
            PortfolioHistory ph = tester.PortfolioHistories.Where(s => Enumerable.SequenceEqual(s.Key, new string[] { asset })).First().Value;
            if (asset.EndsWith("Bond"))
            {
                double finalBondPrice = PortfolioStats.CalculateFinalBondPrice(asset, ph, bondYields[asset], assetConfig[asset].ExRate, assetExchangeRates[asset]);
                assetPrices[asset] = finalBondPrice;
            }
            double allowedRisk;
            if (!double.TryParse(riskAmount, out allowedRisk))
            {
                throw new InvalidOperationException();
            }
            IRecommendationEngine rec = tester.RiskEngine;
            double ltrr = ph.LTRR[asset];
            assetPrices.TryGetValue(asset, out double assetPrice);
            double riskRatio = rec.CalculateRiskRatio(MoneySupply.CalculateM2AdjustedToAsset(DateTime.Now.Year - StartYear, ltrr), assetPrice, ph.AssetAverageRisks[asset], ph.AverageM2toPriceRatios[asset], ph.M2toPriceAmplitude[asset], ph.AssetIncomeRate[asset]);
            assetPositions.TryGetValue(asset, out double position);
            amountToInvest = rec.FindRecommendedPosition(position, allowedRisk, riskRatio, assetCombination.Count);
            amountRisked = amountToInvest * riskRatio;
        }

        private static void LoadState()
        {
            if (File.Exists("State.json"))
            {
                GUI.State state = JsonConvert.DeserializeObject<GUI.State>(File.ReadAllText("State.json"));
                assetCombination = state.AssetCombination;
                assetPrices = state.AssetPrices;
                assetPositions = state.AssetPositions;
                assetExchangeRates = state.AssetExchangeRates;
                bondYields = state.BondYields;
                riskAmount = state.RiskAmount ?? string.Empty;
            }
        }

        static void Main(string[] args)
        {
            assetConfig = JsonConvert.DeserializeObject<Dictionary<string, AssetConfig>>(File.ReadAllText("Data/AssetConfig.json"));
            tester.IntegrationTest_OptimizeAssetCombinations(
                null, null
                );
            LoadState();
            portfolio = new AssetPortfolio();
            portfolio.Assets = new List<Asset>();
            if (assetCombination.Count <= 0)
            {
                System.Console.WriteLine("Please select a combination of assets.");
                return;
            }
            //test

            try
            {
                foreach (string assetType in assetCombination)
                {
                    CalculateMetrics(assetType, out double amountToInvest, out double amountRisked);
                    portfolio.Assets.Add(new Asset { Type = assetType, AmountInvested = amountToInvest, AmountRisked = amountRisked });
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
                return;
            }
            if (!double.TryParse(riskAmount, out double allowedRisk) || allowedRisk <= 0)
            {
                System.Console.WriteLine("Please enter your allowed risk in $ (e.g. 10000).", "Require Allowed Risk > 0");
                return;
            }

            FetchBondYields();

            portfolio.BalanceAllocationsForMaxRisk(allowedRisk);

            tester.CreateReport(ReportType.HTML, portfolio, assetPrices, assetCombination, riskAmount, tester.PortfolioHistories.Where(s => Enumerable.SequenceEqual(s.Key, assetCombination)).First().Value.Stats.StatText, assetPositions);
            //System.Diagnostics.Process.Start("Report.html");
        }

        private static void FetchBondYields()
        {
            var bondAssets = portfolio.Assets.Where(a => a.Type.ToUpperInvariant().EndsWith("BOND"));
            if (bondAssets.Any())
            {
                foreach (var asset in bondAssets)
                {
                    var yields = new List<double>();
                    var csvAnnual = string.Empty;
                    var csvMonthly = string.Empty;
                    try
                    {
                        if (asset.Type == "CABond")
                        {
                            try
                            {
                                double exchangeRate = AssetUtils.FetchExchangeRate("USD", "CAD");
                                assetExchangeRates[asset.Type] = exchangeRate;
                            }
                            catch (Exception ex)
                            {
                                System.Console.WriteLine("Error fetching exchange rate -- " + ex.ToString());
                            }
                        }
                        AssetUtils.FetchBondYields(
                            assetConfig[asset.Type],
                            out yields,
                            out csvAnnual,
                            out csvMonthly
                        );
                        bondYields[asset.Type] = yields;
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine("Error fetching bond yields; using existing data -- " + ex.ToString());
                    }
                }
            }
        }
    }
}
