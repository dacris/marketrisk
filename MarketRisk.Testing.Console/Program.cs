using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.Testing.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            new Tester().IntegrationTest_OptimizeAssetCombinations(
                ConfigurationManager.AppSettings["MaxSalePercent"],
                ConfigurationManager.AppSettings["RealRiskGrowthRatePercent"]);
            System.Console.WriteLine("All done. Press any key to exit.");
            System.Console.ReadKey();
        }
    }
}
