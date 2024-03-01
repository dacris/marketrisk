using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.Recommend.Planning
{
    public class PlanInput
    {
        [Category("Current Stats")]
        [DisplayName("Current Savings Amount")]
        public double? CurrentSavingsAmount { get; set; }
        [Category("Current Stats")]
        [DisplayName("Current Net Income")]
        public double? NetIncome { get; set; }
        [Category("Current Stats")]
        [DisplayName("Current Invested Amount")]
        public double? CurrentInvestedAmount { get; set; }
        [Category("Future Projections")]
        [DisplayName("Rate of Return on Savings (%/yr)")]
        public double? RateOfReturnOnSavings { get; set; }
        [Category("Future Projections")]
        [DisplayName("Rate of Return on Investments (%/yr)")]
        public double? RateOfReturnOnInvestments { get; set; }
        [Category("Future Projections")]
        [DisplayName("Rate of Inflation (%/yr)")]
        public double InflationRate { get; set; } = 3.8;
        [Category("Goal")]
        [DisplayName("Years Until Accomplishment")]
        public int NumberOfYears { get; set; } = 5;
        [Category("Goal")]
        [DisplayName("1 - Goal Type")]
        [TypeConverter(typeof(GoalTypeConverter))]
        public int TypeOfGoal { get; set; }
        [Category("Goal")]
        [DisplayName("2 - Goal Amount Or Income")]
        [Description("Income in retirement, or purchase total (Present Value)")]
        public double? GoalAmountOrIncome { get; set; }
        [Category("Goal")]
        [DisplayName("Percent of Goal Risked")]
        [Description("Risk tolerance as a percent of your goal amount")]
        public double PercentRisked { get; set; } = 25;
        [Category("Goal")]
        [DisplayName("Margin of Safety (%)")]
        public double MarginOfSafety { get; set; } = 10;
    }

    public static class GoalType
    {
        [Description("Retirement Income")]
        public const int RetirementIncome = 0;
        [Description("Major Purchase")]
        public const int MajorPurchase = 1;
    }

    public class GoalTypeConverter : Int32Converter
    {
        private List<string> values = new List<string>();

        public GoalTypeConverter()
        {
            values.Add("Retirement Income");
            values.Add("Major Purchase");
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is int)
            {
                int index = (int)value;
                if (index >= 0 && index < values.Count)
                    return values[index];

                return values[0]; // error, go back to first
            }
            return value;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string s = value as string;
            if (s != null)
            {
                int index = values.IndexOf(s);
                if (index >= 0)
                    return index;

                // support direct integer input & validate
                if (int.TryParse(s, out index) && index >= 0 && index < values.Count)
                    return index;

                return 0; // error, go back to first
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(values);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
