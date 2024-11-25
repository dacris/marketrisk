using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketRisk.Portfolio
{
    public class Asset
    {
        public string Type { get; set; }
        public double AmountInvested { get; set; }
        public double AmountRisked { get; set; }
    }
}