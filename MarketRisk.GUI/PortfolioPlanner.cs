using MarketRisk.Recommend.Planning;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarketRisk.GUI
{
    public partial class PortfolioPlanner : Form
    {
        public PlanInput Input { get { return (PlanInput)propertyGrid1.SelectedObject; } set { propertyGrid1.SelectedObject = value; propertyGrid1_PropertyValueChanged(this, new PropertyValueChangedEventArgs(null, null)); } }
        public PortfolioPlanner()
        {
            InitializeComponent();
        }

        private void PortfolioPlanner_Load(object sender, EventArgs e)
        {
            propertyGrid1.Refresh();
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            PlanCalculator planCalculator = new PlanCalculator();
            planCalculator.Calculate(Input);
            if (planCalculator.HasValue)
            {
                textBox1.Text = planCalculator.ToString();
            }
        }
    }
}
