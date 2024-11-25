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
        [Category("Goal")]
        [TypeConverter(typeof(GoalTypeConverter))]
        public GoalType Goal { get; set; }
        [Category("Goal")]
        [DisplayName("Goal Amount Or Income")]
        [Description("Income in retirement, or purchase total (Present Value)")]
        public double? GoalAmountOrIncome { get; set; }
        [Category("Goal")]
        [DisplayName("Percent of Goal Risked")]
        [Description("Risk tolerance as a percent of your goal amount")]
        public double PercentRisked { get; set; } = 25;
        [Category("Goal")]
        [DisplayName("Margin of Safety (%)")]
        public double MarginOfSafety { get; set; } = 10;
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
    }

    public enum GoalType
    {
        [Description("Retirement Income")]
        RetirementIncome=0,
        [Description("Major Purchase")]
        MajorPurchase=1
    }

    public class GoalTypeConverter : EnumConverter
    {
        private Type enumType;

        public GoalTypeConverter(Type type) : base(type)
        {
            enumType = type;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
        {
            return destType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
                                         object value, Type destType)
        {
            if (value == null)
                return null;

            FieldInfo fi = enumType.GetField(Enum.GetName(enumType, value));
            DescriptionAttribute dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi,
                                        typeof(DescriptionAttribute));
            if (dna != null)
                return dna.Description;
            else
                return value.ToString();
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            return srcType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture,
                                           object value)
        {
            foreach (FieldInfo fi in enumType.GetFields())
            {
                DescriptionAttribute dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi,
                                            typeof(DescriptionAttribute));
                if ((dna != null) && ((string)value == dna.Description))
                    return Enum.Parse(enumType, fi.Name);
            }
            return Enum.Parse(enumType, (string)value);
        }
    }
}
