using MarketRisk.Testing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarketRisk.GUI
{
    public partial class Bond : UserControl
    {
        public AssetConfig AssetConfig { get; set; }
        public string ExchangeRateDisplay { get { return null; } set { label3.Text = label3.Text.Replace("CAD/USD", value); } }
        public bool ShowExchangeRate { get { return label3.Visible; } set { label3.Visible = value; textBox3.Visible = value; } }
        public string Prompt { set { label1.Text = value; } }
        public string[] Input { get { return textBox1.Lines; } set { textBox1.Lines = value; } }
        public string PositionSize { get { return textBox2.Text; } set { textBox2.Text = value; } }
        public double ExchangeRate { get { return double.TryParse(textBox3.Text, out double r) ? r : 1.0; } set { textBox3.Text = value.ToString("N3"); } }
        public Bond()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                List<double> bondYields;
                string csvAnnual, csvMonthly;
                AssetUtils.FetchBondYields(AssetConfig, out bondYields, out csvAnnual, out csvMonthly);
                Input = bondYields.Select(yld => yld.ToString("N2")).ToArray();
#if DEBUG
                System.IO.File.WriteAllText("DatasetAnnual.csv", csvAnnual);
                System.IO.File.WriteAllText("DatasetMonthly.csv", csvMonthly);
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.ToString()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }
}
