using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.Portfolio
{
    public class PortfolioStats
    {
		public List<double> AnnualReturn { get; set; }
		public double Last50Yr_ChangeInPrice { get; set; }
		public double Last50Yr_LowestReturn { get; set; }
		public double Last50Yr_AnnualizedRateOfReturn { get { return Math.Pow(Last50Yr_ChangeInPrice, 1.0 / 50.0); } }
		public double MaxAmountRisked { get; set; }
        public double Average_50Yr_AnnualReturnVariance { get; set; }

        public static double CalculateFinalBondPrice(string asset, PortfolioHistory ph, List<double> bondYields, double exRateStart, double exRateEnd)
        {
            // Calculate the price here
            double finalBondPrice = ph.AssetPriceHistory[asset][ph.AssetPriceHistory[asset].Count - 1];
            double prevPrice = 0;
            double price = 0;
            double prevYield = 0;
            int year = 0;
            foreach (double yield in bondYields)
            {
                if (year >= 1)
                {
                    price = 100.0 / Math.Pow(1.0 + yield / 100.0, 10.0);
                    finalBondPrice = finalBondPrice * (price / prevPrice) * ((100.0 + prevYield) / 100.0);
                }
                year++;
                prevPrice = 100.0 / Math.Pow(1.0 + yield / 100.0, 10.0);
                prevYield = yield;
            }

            return finalBondPrice / (exRateEnd / exRateStart);
        }

        public string StatText
        {
            get
            {
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("DEMO PORTFOLIO STATS");
				sb.AppendLine(string.Format("50 Years Growth: {0:N2} X", Last50Yr_ChangeInPrice));
				sb.AppendLine(string.Format("50 Years Lowest Return: {0:N2} %", (Last50Yr_LowestReturn - 1.0) * 100.0));
				sb.AppendLine(string.Format("50 Years Annualized Rate of Return: {0:N2} %", (Last50Yr_AnnualizedRateOfReturn - 1.0) * 100.0));
                sb.AppendLine(string.Format("Average Rolling 50-Year Return Variance: +/- {0:N2} %", Average_50Yr_AnnualReturnVariance));
                sb.AppendLine(string.Format("Maximum Amount Risked: ${0:N2}", MaxAmountRisked));
				return sb.ToString();
			}
		}

		public void OutputToConsole()
		{
			Console.Write(StatText);
		}

        public string OutputToJSON()
        {
            return "{\"Growth\":" + string.Format("{0:N2}", Last50Yr_ChangeInPrice).Replace(",", "") +
                ",\"LowestReturn\":" + string.Format("{0:N2}", (Last50Yr_LowestReturn - 1.0) * 100.0) +
                ",\"AnnualizedRateOfReturn\":" + string.Format("{0:N2}", (Last50Yr_AnnualizedRateOfReturn - 1.0) * 100.0) + "}";
        }
    }
}
