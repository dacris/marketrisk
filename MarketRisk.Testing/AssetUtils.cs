using MarketRisk.Portfolio;
using MarketRisk.Recommend;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.Testing
{
    public static class AssetUtils
    {
        private static HttpClient UrlClient = new HttpClient();
        public static void PredictAssetPrice(Tester tester, int startYear, string asset, out double highRisk, out double medRisk, out double lowRisk)
        {
            double highRiskCount = 0;
            double medRiskCount = 0;
            double lowRiskCount = 0;
            highRisk = 0;
            medRisk = 0;
            lowRisk = 0;
            double startPrice = 10000000000;
            double step = 1.04;
            for (double price = startPrice; price >= 0.05; price = price / step)
            {
                IRecommendationEngine rec = tester.RiskEngine;
                PortfolioHistory ph = tester.PortfolioHistories.Where(s => Enumerable.SequenceEqual(s.Key, new string[] { asset })).First().Value;
                double ltrr = ph.LTRR[asset];
                double riskRatio = rec.CalculateRiskRatio(MoneySupply.CalculateM2AdjustedToAsset(DateTime.Now.Year - startYear, ltrr), price, ph.AssetAverageRisks[asset], ph.AverageM2toPriceRatios[asset], ph.M2toPriceAmplitude[asset], ph.AssetIncomeRate[asset]);
                if (riskRatio >= 0.91 && riskRatio <= 0.96)
                {
                    highRisk += price;
                    highRiskCount++;
                }
                if (riskRatio >= 0.83 && riskRatio < 0.91)
                {
                    medRisk += price;
                    medRiskCount++;
                }
                if (riskRatio >= 0.2 && riskRatio < 0.83)
                {
                    lowRisk += price;
                    lowRiskCount++;
                }
            }
            highRisk = highRisk / highRiskCount;
            medRisk = medRisk / medRiskCount;
            lowRisk = lowRisk / lowRiskCount;
        }

        public static void FetchBondYields(AssetConfig assetConfig, out List<double> bondYields, out string csvAnnual, out string csvMonthly)
        {
            bondYields = new List<double>();
            csvAnnual = UrlClient.GetStringAsync(assetConfig.DatasetUrlAnnual.Replace("2021-01-01", (DateTime.Now.Year - 2).ToString() + "-01-01").Replace("2023-01-03", DateTime.Today.ToString("yyyy-MM-dd"))).Result;
            csvMonthly = UrlClient.GetStringAsync(assetConfig.DatasetUrlMonthly.Replace("2022-01-01", (DateTime.Now.Year - 1).ToString() + "-01-01").Replace("2022-11-01", (DateTime.Now.Year - 1).ToString() + "-11-01").Replace("2023-01-03", DateTime.Today.ToString("yyyy-MM-dd"))).Result;
            CSVHelper annualRecords = new CSVHelper(csvAnnual);
            CSVHelper monthlyRecords = new CSVHelper(csvMonthly);
            bondYields.AddRange(annualRecords.Skip(1).Select(s => double.TryParse(s[1], out double yld) ? yld : 0));
            bondYields.Add(monthlyRecords.Skip(1).Select(s => double.TryParse(s[1], out double yld) ? yld : 0).Average());
        }

        public static double FetchExchangeRate(string domestic, string foreign)
        {
            string json = UrlClient.GetStringAsync(
                $"https://forex-data-feed.swissquote.com/public-quotes/bboquotes/instrument/{domestic}/{foreign}"
            ).Result;
            var start = JArray.Parse(json);
            var matches = start.SelectTokens("[*].spreadProfilePrices[*]");
            return matches.Average(t => double.Parse(t.SelectToken("bid").ToString()));
        }
    }
}
