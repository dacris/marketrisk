using MarketRisk.Portfolio;
using MarketRisk.Testing;
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
    public partial class FindConfig : Form
    {
        //Inputs
        public Tester Tester { get; set; }
        public string[] Assets {
            get {
                List<string> l = new List<string>();
                foreach (string s in checkedListBox1.Items)
                {
                    l.Add(s);
                }
                return l.ToArray();
            }
            set { checkedListBox1.Items.Clear(); checkedListBox1.Items.AddRange(value); numericUpDown1.Maximum = checkedListBox1.Items.Count; }
        }

        //Output
        public string[] BestAssetCombination { get; set; }

        public FindConfig()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //OK
            if (checkedListBox1.Items.Count - checkedListBox1.CheckedItems.Count < numericUpDown1.Value)
            {
                MessageBox.Show($"You excluded too many assets. Can't create a portfolio with {numericUpDown1.Value:N0} assets out of the remaining set. Please un-exclude some assets.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.Cancel;
                return;
            }
            PortfolioHistory bestPH;
            string[] bestAssetCombination;
            Tester.IntegrationTest_OptimizeAssetCombinations_Specific(ListUtils.Combinations(Assets), (int)numericUpDown1.Value, checkedListBox1.CheckedItems.Cast<string>().ToArray(), out bestPH, out bestAssetCombination);
            BestAssetCombination = bestAssetCombination;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Cancel
            Close();
        }
    }
}
