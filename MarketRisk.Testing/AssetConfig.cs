using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MarketRisk.Testing
{
    public class AssetConfig
    {
      public double ExRate {get;set;}
      public string ExRateDisplay {get;set;}
      public string DatasetUrlAnnual {get;set;}
      public string DatasetUrlMonthly {get;set;}
      
      public AssetConfig() {}

/*
      public AssetConfig(double exRate, string exRateDisplay, string datasetUrlAnnual, string datasetUrlMonthly)
      {
        this.ExRate = exRate;
        this.ExRateDisplay = exRateDisplay;
        this.DatasetUrlAnnual = datasetUrlAnnual;
        this.DatasetUrlMonthly = datasetUrlMonthly;
      }
*/
    }
}
