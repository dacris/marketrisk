using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.Portfolio
{
    public class Portfolio
    {
        public List<Asset> Assets { get; set; }

        public void BalanceAllocationsForMaxRisk(double maxRiskAmount)
        {
            BalanceAllocationsForMaxRisk_Complex(maxRiskAmount);
        }

        /// <summary>
        /// Added for reference, to test performance stats accuracy. Not used.
        /// </summary>
        public void BalanceAllocationsForMaxRisk_EqualWeight()
        {
            double totalInvested = Assets.Sum(a => a.AmountInvested);
            foreach (Asset a in Assets)
            {
                double ratio = (totalInvested / Assets.Count) / a.AmountInvested;
                a.AmountInvested *= ratio;
                a.AmountRisked *= ratio;
            }
        }

        // Proven Method to Re-Balance Portfolio for Max Risk:
        // Adjusts amount invested by ratio of total risk to max risk.
        // Post-Condition: Total amount risked = Max risk amount
        public void BalanceAllocationsForMaxRisk_Simple(double maxRiskAmount)
        {
            // Calculate total risk
            double totalRiskAmount = Assets.Sum(a => a.AmountRisked);
            double ratio = maxRiskAmount / totalRiskAmount;
            foreach (Asset a in Assets)
            {
                a.AmountInvested *= ratio;
                a.AmountRisked *= ratio;
            }
        }

        // *** New in Build 9 ***
        // Adjusts amount invested by ratio of invested to risked.
        // Gives slightly better (+0.6%) return with stocks but higher downside risk (+4%) with PMs.
        // Post-Condition: Total amount risked = Max risk amount
        public void BalanceAllocationsForMaxRisk_Complex(double maxRiskAmount)
        {
            // Calculate total risk
            double totalRiskAmount = Assets.Sum(a => a.AmountRisked);
            double ratio = maxRiskAmount / totalRiskAmount;
            double totalRiskRatio = totalRiskAmount / Assets.Sum(a => a.AmountInvested);
            foreach (Asset a in Assets)
            {
                double riskRatio = a.AmountRisked / a.AmountInvested;
                double exponent = 1.1; // lower = lower risk, lower return //1.0 seems to work better?
                // Changes in Build 15:
                // Use Pow(ratio, 1/N) instead of ratio / N, makes more sense from first principles
                // No need to decrease linearly with asset count
                double multiplier = Math.Pow(ratio, 1 / Math.Pow((riskRatio / totalRiskRatio), exponent));
                double maxRiskConcentration = Math.Max(0.8, (1.0 / Assets.Count) * 2.5); //80% for 1-3 assets, 62.5% for 4 assets, 50% for 5 assets, etc.
                // New in build 26:
                // Avoid risk concentration of > 80% in one asset
                if (a.AmountRisked * multiplier > maxRiskConcentration * totalRiskAmount * ratio)
                {
                    multiplier = maxRiskConcentration * totalRiskAmount * ratio / a.AmountRisked;
                }
                a.AmountInvested *= multiplier;
                a.AmountRisked *= multiplier;
            }
            BalanceAllocationsForMaxRisk_Simple(maxRiskAmount);
        }
    }
}
