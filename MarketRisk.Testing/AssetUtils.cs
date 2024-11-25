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
            double medRiskCount = 1;
            double lowRiskCount = 0;
            highRisk = 0;
            medRisk = 0;
            lowRisk = 0;
            PortfolioHistory ph = tester.PortfolioHistories.Where(s => Enumerable.SequenceEqual(s.Key, new string[] { asset })).First().Value;
            double ltrr = ph.LTRR[asset];
            double m2 = MoneySupply.CalculateM2AdjustedToAsset(DateTime.Now.Year - startYear, ltrr);
            double startPrice = Math.Pow(ph.M2toPriceAmplitude[asset], 2.0) * m2 / ph.AverageM2toPriceRatios[asset];
            medRisk = m2 / ph.AverageM2toPriceRatios[asset];
            double step = 1.002;
            for (double price = startPrice; price >= 0.01; price = price / step)
            {
                IRecommendationEngine rec = tester.RiskEngine;
                double riskRatio = rec.CalculateRiskRatio(m2, price, ph.AssetAverageRisks[asset], ph.AverageM2toPriceRatios[asset], ph.M2toPriceAmplitude[asset], ph.AssetIncomeRate[asset]);
                if (riskRatio >= 0.92 && riskRatio <= 0.98)
                {
                    highRisk += price;
                    highRiskCount++;
                }
                if (riskRatio >= 0.83 && riskRatio < 0.92)
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
            lowRisk = lowRisk / lowRiskCount;
            medRisk = (highRisk + lowRisk + medRisk + medRisk + medRisk) / (medRiskCount * 3.0 + 2.0);
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
