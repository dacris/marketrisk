using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.Testing
{
    public class AssetConfig
    {
        public double ExRate { get; set; }
        public string ExRateDisplay { get; set; }
        public string DatasetUrlAnnual { get; set; }
        public string DatasetUrlMonthly { get; set; }
    }
}
