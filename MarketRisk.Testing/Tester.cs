using MarketRisk.Portfolio;
using MarketRisk.Recommend;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MarketRisk.Testing
{
    public class Tester
    {
        public double StartingRiskToleranceAmountTotal { get; set; } = 1000;
        public double InflationRate { get; set; } = 1.038;
        public double RealRiskGrowthRate { get; set; } = 0.04;
        public Dictionary<string[], PortfolioHistory> PortfolioHistories { get; set; } = new Dictionary<string[], PortfolioHistory>();
        public string[] BestAssetCombination { get; set; } = null;
        public IEnumerable<string> Assets { get; set; }
        public IRecommendationEngine RiskEngine { get; set; }
        public void IntegrationTest_OptimizeAssetCombinations_Specific(IEnumerable<string[]> combinations, int numAssets, string[] excludedAssets, out PortfolioHistory bestPH, out string[] bestAssetCombination)
        {
            bestPH = null;
            bestAssetCombination = null;
            double maxRiskAdjustedRateOfReturn = 0;
            StringBuilder jsonOut = new StringBuilder();
            foreach (string[] assetCombination in combinations.Where(c => c.Length >= 1 && (numAssets < 1 || c.Length == numAssets) && (excludedAssets == null || !c.Intersect(excludedAssets).Any())))
            {
                Console.WriteLine("Testing Combination [" + string.Join(",", assetCombination).TrimEnd(',') + "]...");
                PortfolioHistory ph = new PortfolioHistory();
                PortfolioHistories[assetCombination] = ph;
                IEnumerable<string[]> assetIncomeRates = File.ReadAllLines("Data/AssetIncome.cfg").Select(l => l.Split(','));
                var dict = assetIncomeRates.Select(s => new Tuple<string, double>(s[0], double.Parse(s[1]))).ToDictionary(k => k.Item1, k => k.Item2);
                ph.AssetIncomeRate = new Dictionary<string, double>(dict);
                foreach (string asset in assetCombination)
                {
                    ph.LoadAssetPriceHistory(asset, "Data/" + asset + ".txt");
                }
                ph.AllowedAssets = assetCombination.ToList();
                /*ph.AssetAverageRisks = new Dictionary<string, double>
                {
                    { "Bond", 0.25 }, { "Dow", 0.4 }, { "Gold", 0.25 }, { "Palladium", 0.4 }, { "Silver", 0.5 }
                };*/
                ph.CalculateAllocationHistory(RiskEngine, StartingRiskToleranceAmountTotal, InflationRate, RealRiskGrowthRate);
                ph.CalculateStats();
                ph.Stats.OutputToConsole();
                string comma = ",";
                if (jsonOut.Length == 0) { comma = "{\"Combinations\":["; }
                jsonOut.AppendLine(comma + "{\"AssetCombination\":[" + string.Join(",", assetCombination.Select(s => "\"" + s + "\"")) + "],\"Stats\":"+ph.Stats.OutputToJSON()+"}");
                double riskAdjustedRate = Math.Pow(ph.Stats.Last50Yr_ChangeInPrice, 1.0 / 4.0) * ph.Stats.Last50Yr_LowestReturn;
                if (riskAdjustedRate >= maxRiskAdjustedRateOfReturn)
                {
                    maxRiskAdjustedRateOfReturn = riskAdjustedRate;
                    bestAssetCombination = assetCombination;
                    bestPH = ph;
                }
            }
            jsonOut.Append("]}");
            File.WriteAllText("Combinations.json", jsonOut.ToString());
        }
        public void IntegrationTest_OptimizeAssetCombinations(string maxSalePercentSetting, string realRiskGrowthRatePercentSetting)
        {
            // Test all possible combinations of assets
            RiskEngine = new AlgorithmicRiskRecommendationEngine();
            if (double.TryParse(maxSalePercentSetting, out double maxSale))
            {
                RiskEngine.MaxSale = maxSale / 100.0;
            }

            if (double.TryParse(realRiskGrowthRatePercentSetting, out double rrgrPercent))
            {
                RealRiskGrowthRate = rrgrPercent / 100.0;
            }
            string[] files = Directory.GetFiles("Data", "*.txt").OrderBy(f => f).ToArray();
            Assets = files.Select(file => Path.GetFileNameWithoutExtension(file));
            var combinations = ListUtils.Combinations(Assets);
            PortfolioHistory bestPH = null;
            string[] bestAssetCombination = null;
            IntegrationTest_OptimizeAssetCombinations_Specific(combinations, 0, null, out bestPH, out bestAssetCombination);
            BestAssetCombination = bestAssetCombination;
            Console.WriteLine("*** Best Asset Combination: [" + string.Join(",", BestAssetCombination).TrimEnd(',') + "]");
            bestPH.Stats.OutputToConsole();
#if DEBUG
            int i = 0;
            using (StreamWriter sw = new StreamWriter("Portfolio.csv"))
            {
                foreach (Portfolio.Portfolio p in PortfolioHistories[BestAssetCombination].AllocationHistory)
                {
                    if (i == 0)
                    {
                        foreach (Asset a in p.Assets)
                        {
                            sw.Write(a.Type + ",");
                        }
                        sw.WriteLine();
                    }
                    foreach (Asset a in p.Assets)
                    {
                        sw.Write(a.AmountInvested + ",");
                    }
                    sw.WriteLine();
                    i++;
                }
            }
#endif
        }

        public void CreateReport(ReportType reportType,
            Portfolio.Portfolio portfolio,
            Dictionary<string, double> assetPrices,
            IEnumerable<string> assetCombination,
            string riskTolerance,
            string portfolioStats,
            Dictionary<string, double> assetPositions)
        {
            if (reportType == ReportType.HTML)
            {
                Dictionary<string, double[]> assetMetrics = new Dictionary<string, double[]>();
                foreach (Asset a in portfolio.Assets)
                {
                    assetMetrics.Add(a.Type, new double[] { a.AmountInvested, a.AmountRisked });
                }
                string htmlTemplate = File.ReadAllText("ReportTemplate.html");
                htmlTemplate = htmlTemplate.Replace("@@DATE", DateTime.Today.ToLongDateString());
                htmlTemplate = htmlTemplate.Replace("@@RISK", riskTolerance);
                htmlTemplate = htmlTemplate.Replace("@@PORTFOLIO_STATS", portfolioStats);
                StringBuilder sbAssets = new StringBuilder();
                foreach (string asset in assetCombination)
                {
                    string assetPriceStr = "N/A";
                    if (assetPrices.TryGetValue(asset, out double assetPrice))
                    {
                        assetPriceStr = assetPrice.ToString("N2");
                    }
                    sbAssets.AppendLine(string.Format("<tr><td>{0}</td><td>{1:N2}</td><td>{2:N2}</td><td>{3:N2}</td></tr>", asset, assetPriceStr, assetMetrics[asset][0], assetMetrics[asset][1]));
                }
                htmlTemplate = htmlTemplate.Replace("@@ASSET_ROWS", sbAssets.ToString());
                StringBuilder sbETFs = new StringBuilder();
                sbETFs.Append("<ul>");
                var etfDb = JObject.Parse(File.ReadAllText("ETFs.json"));
                foreach (string asset in assetCombination)
                {
                    if (etfDb.ContainsKey(asset))
                    {
                        string assetETFStr = "<li>" + asset + ": " + etfDb[asset]["ETFs"] + " (<a href=\"" + etfDb[asset]["URL"] + "\" target=\"_blank\">More ETFs</a>)</li>";
                        sbETFs.AppendLine(assetETFStr);
                    }
                }
                sbETFs.Append("</ul>");
                htmlTemplate = htmlTemplate.Replace("@@ETFS", sbETFs.ToString());
                File.WriteAllText("Report.html", htmlTemplate);
            }
            else if (reportType == ReportType.CSV)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("MarketRisk Excel Report");
                sb.AppendLine(DateTime.Today.ToString("yyyy-MM-dd"));
                sb.AppendLine();
                sb.AppendLine(string.Format("Total Currently Invested:,{0:F2}", assetPositions.Where(a => portfolio.Assets.Any(u => u.Type == a.Key)).Sum(a => a.Value)));
                sb.AppendLine(string.Format("Total Future Invested:,{0:F2}", portfolio.Assets.Sum(s => s.AmountInvested)));
                sb.AppendLine();
                sb.AppendLine(string.Format("Total Currently Risked:,{0:F2}", assetPositions.Where(a => portfolio.Assets.Any(u => u.Type == a.Key)).Sum(a => a.Value * portfolio.Assets.Where(u => u.Type == a.Key).First().AmountRisked / portfolio.Assets.Where(u => u.Type == a.Key).First().AmountInvested)));
                sb.AppendLine(string.Format("Total Future Risked:,{0:F2}", portfolio.Assets.Sum(s => s.AmountRisked)));
                sb.AppendLine();
                sb.AppendLine(string.Format("Risk Tolerance:,{0}", riskTolerance.Replace(",", "")));
                sb.AppendLine();
                sb.Append("Asset Allocation:,");
                sb.Append(string.Join(",", assetCombination.OrderBy(a => a)));
                sb.AppendLine();
                sb.Append("Current Position:,");
                sb.Append(string.Join(",", assetPositions.Where(a => portfolio.Assets.Any(u => u.Type == a.Key)).OrderBy(a => a.Key).Select(kvp => kvp.Value.ToString("F2"))));
                sb.AppendLine();
                sb.Append("Future Amount Invested:,");
                sb.Append(string.Join(",", portfolio.Assets.Select(a => a.AmountInvested.ToString("F2"))));
                sb.AppendLine();
                sb.Append("Future Amount Risked:,");
                sb.Append(string.Join(",", portfolio.Assets.Select(a => a.AmountRisked.ToString("F2"))));
                sb.AppendLine();
                sb.Append("Market Price:,");
                sb.Append(string.Join(",", assetPrices.Where(a => portfolio.Assets.Any(u => u.Type == a.Key)).OrderBy(a => a.Key).Select(a => a.Value.ToString("F2"))));
                File.WriteAllText("Report.csv", sb.ToString());
            }
        }
    }

    public enum ReportType
    {
        CSV,
        HTML
    }
}
