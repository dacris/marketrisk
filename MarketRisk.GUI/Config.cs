using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.GUI
{
    class Config
    {
        public static readonly int EndYear = int.Parse(ConfigurationManager.AppSettings["EndYear"]);
        public static readonly int StartYear = int.Parse(ConfigurationManager.AppSettings["StartYear"]);
    }
}
