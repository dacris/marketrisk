using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;

namespace MarketRisk.Portfolio
{
    public class Asset
    {
        public string Type { get; set; }
        public double AmountInvested { get; set; }
        public double AmountRisked { get; set; }
    }
}