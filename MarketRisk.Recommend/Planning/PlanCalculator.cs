using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.Recommend.Planning
{
    public class PlanCalculator
    {
        private const double RiskMinLevel = 5;
        private const double RiskWarningLevel = 40;
        private const double RiskErrorLevel = 50;

        public bool HasValue { get; set; }
        public double AmountSaved { get; set; }
        public double AmountInvested { get; set; }
        public double AnnualSaved { get; set; }
        public double AnnualInvested { get; set; }
        public double YearsOfSavings { get; set; }
        private PlanInput Input { get; set; }

        public void Calculate(PlanInput input)
        {
            Input = input;

            // Validation
            if (!input.CurrentInvestedAmount.HasValue || !input.RateOfReturnOnInvestments.HasValue ||
                !input.RateOfReturnOnSavings.HasValue || !input.CurrentSavingsAmount.HasValue ||
                !input.GoalAmountOrIncome.HasValue)
            {
                return;
            }
            if (input.NumberOfYears <= 0)
            {
                return;
            }
            if (input.GoalAmountOrIncome <= 0)
            {
                return;
            }
            if (input.PercentRisked < RiskMinLevel)
            {
                input.PercentRisked = RiskMinLevel;
            }
            if (input.PercentRisked > RiskErrorLevel)
            {
                input.PercentRisked = RiskErrorLevel;
            }

            double toleranceOfRisk = input.PercentRisked / 100.0;
            double investmentRisk = 0.5;
            if (input.Goal == GoalType.RetirementIncome)
            {
                // 1. Determine the amounts we require (PV)
                double goalAmount = input.GoalAmountOrIncome.Value;
                double maturityAmount = input.GoalAmountOrIncome.Value / (0.01 * (input.RateOfReturnOnInvestments.Value - input.InflationRate - 2.0)); // Safe withdrawal rate = ROR - Inflation - 2%
                double rateOfSavingsDepreciation = 1 + (input.InflationRate - Math.Min(input.RateOfReturnOnSavings.Value, input.InflationRate - 0.25)) / 100.0;
                double nYearsToMaturity = Math.Log(maturityAmount / ((toleranceOfRisk / investmentRisk) * goalAmount)) / Math.Log(1 + 0.01 * (input.RateOfReturnOnInvestments.Value - input.InflationRate));
                double nYearsOfSavings = Math.Log(1 - (1 - toleranceOfRisk / investmentRisk) * goalAmount * (1 - rateOfSavingsDepreciation) / input.GoalAmountOrIncome.Value) / Math.Log(rateOfSavingsDepreciation);
                while (nYearsToMaturity > nYearsOfSavings)
                {
                    goalAmount *= 1.025;
                    nYearsToMaturity = Math.Log(maturityAmount / ((toleranceOfRisk / investmentRisk) * goalAmount)) / Math.Log(1 + 0.01 * (input.RateOfReturnOnInvestments.Value - input.InflationRate));
                    nYearsOfSavings = Math.Log(1 - (1 - toleranceOfRisk / investmentRisk) * goalAmount * (1 - rateOfSavingsDepreciation) / input.GoalAmountOrIncome.Value) / Math.Log(rateOfSavingsDepreciation);
                    if (nYearsOfSavings == double.NaN || nYearsToMaturity == double.NaN)
                    {
                        goalAmount = double.NaN;
                    }
                }
                AmountInvested = (input.MarginOfSafety * 0.01 + 1) * goalAmount * toleranceOfRisk / investmentRisk;
                AmountSaved = (input.MarginOfSafety * 0.01 + 1) * goalAmount * (1 - toleranceOfRisk / investmentRisk);
                YearsOfSavings = Math.Log(1 - AmountSaved * (1 - rateOfSavingsDepreciation) / input.GoalAmountOrIncome.Value) / Math.Log(rateOfSavingsDepreciation);
                HasValue = true;
            }
            else if (input.Goal == GoalType.MajorPurchase)
            {
                // 1. Determine the amounts we require (PV)
                AmountInvested = (input.MarginOfSafety * 0.01 + 1) * input.GoalAmountOrIncome.Value * toleranceOfRisk / investmentRisk;
                AmountSaved = (input.MarginOfSafety * 0.01 + 1) * input.GoalAmountOrIncome.Value * (1 - toleranceOfRisk / investmentRisk);
                HasValue = true;
            }
            if (AmountInvested == double.NaN || AmountSaved == double.NaN)
            {
                return;
            }
            // 2. Calculate how we will get there
            double diffSav = AmountSaved - input.CurrentSavingsAmount.Value;
            double diffInv = AmountInvested - input.CurrentInvestedAmount.Value;
            AnnualSaved = FindAnnualAmount(input, diffSav, input.CurrentSavingsAmount.Value, AmountSaved, 1 + 0.01 * (input.RateOfReturnOnSavings.Value - input.InflationRate));
            AnnualInvested = FindAnnualAmount(input, diffInv, input.CurrentInvestedAmount.Value, AmountInvested, 1 + 0.01 * (input.RateOfReturnOnInvestments.Value - input.InflationRate));
        }

        private double FindAnnualAmount(PlanInput input, double diff, double start, double end, double ror)
        {
            double annual = diff / (100 * input.NumberOfYears);
            double current = start;
            double result = current;
            while (result < end)
            {
                for (int yr = 1; yr <= input.NumberOfYears; yr++)
                {
                    current = current * ror + annual / Math.Pow(1 + 0.01 * input.InflationRate, yr);
                }
                result = current;
                current = start;
                annual *= 1.025;
            }
            return annual;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (AmountInvested == double.NaN || AmountSaved == double.NaN)
            {
                sb.AppendLine("Oops. Something went wrong. Try adjusting the parameters.");
                return sb.ToString();
            }
            sb.AppendLine($"Final Amount Saved*: ${AmountSaved:N2}");
            sb.AppendLine($"Final Amount Invested*: ${AmountInvested:N2}");
            sb.AppendLine($"Annual Savings Contribution: ${AnnualSaved:N2}");
            sb.AppendLine($"Annual Investment Contribution: ${AnnualInvested:N2}");
            if (Input.Goal == GoalType.RetirementIncome)
            {
                sb.AppendLine("Spend your savings first.");
                sb.AppendLine($"Savings will last {YearsOfSavings:N1} years.");
            }
            if (Input.NetIncome.HasValue)
            {
                if (Input.NetIncome.Value < AnnualSaved + AnnualInvested)
                {
                    sb.AppendLine("Your goal is in jeopardy! Your net income is too low for your annual contribution. Try increasing the length of time or increasing your income.");
                }
            }
            if (Input.PercentRisked >= RiskWarningLevel)
            {
                sb.AppendLine("You are risking a lot. You can still reach your goal with less risk.");
            }
            if (AnnualSaved <= 0 && AnnualInvested <= 0)
            {
                sb.AppendLine("Try a bigger goal. You've already achieved this one.");
            }
            sb.AppendLine("* Final amounts are present value.");
            return sb.ToString();
        }
    }
}
