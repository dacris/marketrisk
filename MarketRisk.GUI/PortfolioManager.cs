using MarketRisk.Portfolio;
using AssetPortfolio = MarketRisk.Portfolio.Portfolio;
using MarketRisk.Recommend;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.Configuration;
using MarketRisk.Recommend.Planning;
using Newtonsoft.Json.Linq;
using MarketRisk.Testing;

namespace MarketRisk.GUI
{
    public partial class PortfolioManager : Form
    {
        private readonly Tester tester = new Tester();
        private List<ToolStripMenuItem> assetMenuItems = new List<ToolStripMenuItem>();
        private List<string> assetCombination = new List<string>();
        private Dictionary<string, double> assetPrices = new Dictionary<string, double>();
        private Dictionary<string, double> assetPositions = new Dictionary<string, double>();
        private Dictionary<string, double> assetExchangeRates = new Dictionary<string, double>();
        private Dictionary<string, List<double>> bondYields = new Dictionary<string, List<double>>();
        private Dictionary<string, AssetConfig> assetConfig = new Dictionary<string, AssetConfig>();
        private AssetPortfolio portfolio;
        private PortfolioPlanner portfolioPlanner;
        private PlanInput planInput;

        public PortfolioManager()
        {
            InitializeComponent();
            FormClosing += PortfolioManager_FormClosing;
        }

        private void PortfolioManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save state
            SaveState();
        }

        private void SaveState()
        {
            listBox1_SelectedIndexChanged(listBox1, EventArgs.Empty);
            var state = new State { AssetCombination = assetCombination, AssetPrices = assetPrices, AssetPositions = assetPositions, AssetExchangeRates = assetExchangeRates, BondYields = bondYields, RiskAmount = textBox2.Text, PlanInput = planInput };
            File.WriteAllText("State.json", JsonConvert.SerializeObject(state));
        }

        private void LoadState()
        {
            if (File.Exists("State.json"))
            {
                State state = JsonConvert.DeserializeObject<State>(File.ReadAllText("State.json"));
                assetCombination = state.AssetCombination;
                assetPrices = state.AssetPrices;
                assetPositions = state.AssetPositions;
                assetExchangeRates = state.AssetExchangeRates;
                bondYields = state.BondYields;
                textBox2.Text = state.RiskAmount ?? string.Empty;
                portfolioPlanner = new PortfolioPlanner();
                if (state.PlanInput != null)
                {
                    portfolioPlanner.Input = state.PlanInput;
                    planInput = portfolioPlanner.Input;
                }
                if (assetExchangeRates == null)
                {
                    assetExchangeRates = new Dictionary<string, double>();
                }
            }
        }

        private void PortfolioManager_Load(object sender, EventArgs e)
        {
            assetConfig = JsonConvert.DeserializeObject<Dictionary<string, AssetConfig>>(File.ReadAllText("Data\\AssetConfig.json"));
            tester.IntegrationTest_OptimizeAssetCombinations(
                ConfigurationManager.AppSettings["MaxSalePercent"],
                ConfigurationManager.AppSettings["RealRiskGrowthRatePercent"]
                );
            foreach (string asset in tester.Assets)
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem(asset);
                assetsToolStripMenuItem.DropDownItems.Add(menuItem);
                menuItem.Tag = asset;
                menuItem.Checked = false;
                menuItem.Click += AssetItem_Click;
                menuItem.CheckedChanged += MenuItem_CheckedChanged;
                assetMenuItems.Add(menuItem);
            }
            ToolStripMenuItem findBestConfig = new ToolStripMenuItem("&Find Best Configuration...");
            assetsToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            assetsToolStripMenuItem.DropDownItems.Add(findBestConfig);
            findBestConfig.Click += FindBestConfig_Click;
            ToolStripMenuItem bestConfig = new ToolStripMenuItem("&Best Configuration");
            assetsToolStripMenuItem.DropDownItems.Add(bestConfig);
            bestConfig.Click += BestConfig_Click;
            textBox1.Text = @"Disclaimer:
This software can be used to estimate the market risk involved in a specific investment decision.
This software does not provide financial advice.
You should use it only as a rough guideline as part of a broader investment strategy.
We do not, in any way, guarantee that the returns or risks estimated by this software are accurate or will manifest.
Please consult with a qualified financial advisor before making your investment decision.";
            LoadState();
            foreach (string asset in assetCombination)
            {
                assetMenuItems.Where(m => (string)m.Tag == asset).First().Checked = true;
            }
        }

        private void FindBestConfig_Click(object sender, EventArgs e)
        {
            FindConfig configFinder = new FindConfig();
            configFinder.Assets = tester.Assets.ToArray();
            configFinder.Tester = tester;
            if (configFinder.ShowDialog(this) == DialogResult.OK)
            {
                // Select our asset combination
                assetCombination = configFinder.BestAssetCombination.ToList();
                foreach (ToolStripMenuItem item in assetMenuItems)
                {
                    item.Checked = configFinder.BestAssetCombination.Contains((string)item.Tag);
                }
            }
        }

        private void BestConfig_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem menuItem in assetMenuItems)
            {
                bool shouldCheck = tester.BestAssetCombination.Contains((string)menuItem.Tag);
                menuItem.Checked = shouldCheck;
            }
        }

        private void MenuItem_CheckedChanged(object sender, EventArgs e)
        {
            portfolio = null;
            label4.Text = "?";
            label5.Text = "?";
            assetCombination = new List<string>();
            foreach (ToolStripMenuItem menuItem in assetMenuItems)
            {
                if (menuItem.Checked)
                    assetCombination.Add((string)menuItem.Tag);
            }
            listBox1.Items.Clear();
            listBox1.Items.AddRange(assetCombination.ToArray());
            if (assetCombination.Count > 0)
                textBox1.Text = tester.PortfolioHistories.Where(s => Enumerable.SequenceEqual(s.Key, assetCombination)).First().Value.Stats.StatText;
            else
                textBox1.Text = string.Empty;
        }

        private void AssetItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"MarketRisk by Dacris Software\nAssembly Version: {Assembly.GetExecutingAssembly().GetName().Version}\nCopyright \xA9 2023 Dacris Software Inc. All Rights Reserved.\n\nwww.dacris.com", "About MarketRisk", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Make sure we populate bond yields if necessary
            foreach (string asset in listBox1.Items)
            {
                if (asset.EndsWith("Bond"))
                {
                    if (!bondYields.ContainsKey(asset))
                    {
                        bondYields[asset] = new List<double>();
                    }
                }
            }
            if (listBox1.SelectedIndex < 0)
            {
                groupBox1.Controls.Clear();
                return;
            }
            if (listBox1.SelectedIndex >= 0 && groupBox1.Controls.Count > 0)
            {
                if (groupBox1.Controls[0] is Bond)
                {
                    bondYields[(string)groupBox1.Controls[0].Tag] = new List<double>();
                    try
                    {
                        bondYields[(string)groupBox1.Controls[0].Tag].AddRange(((Bond)groupBox1.Controls[0]).Input.Select(yield => double.Parse(yield)));
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show("Please enter numeric bond yields (e.g. 5.25).", "Incorrect Input");
                    }
                    double.TryParse(((Bond)groupBox1.Controls[0]).PositionSize, out double position);
                    assetPositions[(string)groupBox1.Controls[0].Tag] = position;
                    assetExchangeRates[(string)groupBox1.Controls[0].Tag] = ((Bond)groupBox1.Controls[0]).ExchangeRate;
                }
                else
                {
                    double.TryParse(((SinglePrice)groupBox1.Controls[0]).Input, out double price);
                    assetPrices[(string)((SinglePrice)groupBox1.Controls[0]).Tag] = price;
                    double.TryParse(((SinglePrice)groupBox1.Controls[0]).PositionSize, out double position);
                    assetPositions[(string)((SinglePrice)groupBox1.Controls[0]).Tag] = position;
                    if (price <= 0)
                    {
                        MessageBox.Show("Please enter price > $0 for " + (string)((SinglePrice)groupBox1.Controls[0]).Tag, "Incorrect Price");
                    }
                    if (position < 0)
                    {
                        MessageBox.Show("Please enter position size >= $0 for " + (string)((SinglePrice)groupBox1.Controls[0]).Tag, "Incorrect Position Size");
                    }
                }
            }
            groupBox1.Controls.Clear();
            if (((string)listBox1.SelectedItem).EndsWith("Bond"))
            {
                groupBox1.Controls.Add(new Bond());
                ((Bond)groupBox1.Controls[0]).Tag = (string)listBox1.SelectedItem;
                ((Bond)groupBox1.Controls[0]).AssetConfig = assetConfig[(string)listBox1.SelectedItem];
                ((Bond)groupBox1.Controls[0]).Prompt = $"Enter every year's 10-year Bond yield (%/yr) starting in {Config.EndYear} below (one year per line):";
                ((Bond)groupBox1.Controls[0]).Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
                ((Bond)groupBox1.Controls[0]).Top = 24;
                ((Bond)groupBox1.Controls[0]).Left = 12;
                ((Bond)groupBox1.Controls[0]).Width = groupBox1.Width - 24;
                ((Bond)groupBox1.Controls[0]).Height = groupBox1.Height - 32;
                ((Bond)groupBox1.Controls[0]).Input = bondYields[(string)listBox1.SelectedItem].Select(d => d.ToString("N2")).ToArray();
                assetPositions.TryGetValue((string)listBox1.SelectedItem, out double position);
                if (!assetExchangeRates.TryGetValue((string)listBox1.SelectedItem, out double exchangeRate))
                {
                    exchangeRate = 1.0;
                }
                ((Bond)groupBox1.Controls[0]).PositionSize = position.ToString("N2");
                ((Bond)groupBox1.Controls[0]).ExchangeRate = exchangeRate;
                if ((string)listBox1.SelectedItem == "CABond")
                {
                    // Show foreign exchange rate prompt
                    ((Bond)groupBox1.Controls[0]).ShowExchangeRate = true;
                    ((Bond)groupBox1.Controls[0]).ExchangeRateDisplay = assetConfig[(string)listBox1.SelectedItem].ExRateDisplay;
                }
                else
                {
                    // Hide foreign exchange rate prompt
                    ((Bond)groupBox1.Controls[0]).ShowExchangeRate = false;
                }
            }
            else
            {
                groupBox1.Controls.Add(new SinglePrice());
                ((SinglePrice)groupBox1.Controls[0]).Top = 24;
                ((SinglePrice)groupBox1.Controls[0]).Left = 12;
                ((SinglePrice)groupBox1.Controls[0]).Tag = (string)listBox1.SelectedItem;
                if (assetPrices.ContainsKey((string)listBox1.SelectedItem))
                {
                    double price = assetPrices[(string)listBox1.SelectedItem];
                    ((SinglePrice)groupBox1.Controls[0]).Input = price.ToString("N2");
                }
                assetPositions.TryGetValue((string)listBox1.SelectedItem, out double position);
                ((SinglePrice)groupBox1.Controls[0]).PositionSize = position.ToString("N2");
                ((SinglePrice)groupBox1.Controls[0]).PredictPrice += PortfolioManager_PredictPrice;
            }
            if (portfolio != null)
            {
                string asset = (string)listBox1.SelectedItem;
                if (!portfolio.Assets.Any(a => a.Type == asset))
                    return;
                label4.Text = portfolio.Assets.Where(a => a.Type == asset).First().AmountInvested.ToString("N2");
                label5.Text = portfolio.Assets.Where(a => a.Type == asset).First().AmountRisked.ToString("N2");
            }
        }

        private void PortfolioManager_PredictPrice(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
            {
                MessageBox.Show("Please select an asset.", "Asset Price", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            double highRisk, medRisk, lowRisk;
            Testing.AssetUtils.PredictAssetPrice(tester, Config.StartYear, (string)listBox1.SelectedItem, out highRisk, out medRisk, out lowRisk);
            string msg = string.Format("Low Risk Price: {0:N2}\nMedium Risk Price: {1:N2}\nHigh Risk Price: {2:N2}", lowRisk, medRisk, highRisk);
            MessageBox.Show(msg, "Quoted Fair Price", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            portfolio = new AssetPortfolio();
            portfolio.Assets = new List<Asset>();
            listBox1_SelectedIndexChanged(sender, e);
            double allowedRisk;
            if (!double.TryParse(textBox2.Text, out allowedRisk) || allowedRisk <= 0)
            {
                MessageBox.Show("Please enter your allowed risk in $ (e.g. 10000).", "Require Allowed Risk > 0");
                return;
            }
            try
            {
                foreach (string assetType in assetCombination)
                {
                    CalculateMetrics(assetType, out double amountToInvest, out double amountRisked);
                    portfolio.Assets.Add(new Asset { Type = assetType, AmountInvested = amountToInvest, AmountRisked = amountRisked });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return;
            }
            portfolio.BalanceAllocationsForMaxRisk(allowedRisk);
            if (listBox1.SelectedIndex < 0)
                return;
            string asset = (string)listBox1.SelectedItem;
            label4.Text = portfolio.Assets.Where(a => a.Type == asset).First().AmountInvested.ToString("N2");
            label5.Text = portfolio.Assets.Where(a => a.Type == asset).First().AmountRisked.ToString("N2");
        }

        private void CalculateMetrics(string asset, out double amountToInvest, out double amountRisked)
        {
            PortfolioHistory ph = tester.PortfolioHistories.Where(s => Enumerable.SequenceEqual(s.Key, new string[] { asset })).First().Value;
            if (asset.EndsWith("Bond"))
            {
                // Calculate the price here
                double finalBondPrice = PortfolioStats.CalculateFinalBondPrice(asset, ph, bondYields[asset], assetConfig[asset].ExRate, assetExchangeRates[asset]);
                assetPrices[asset] = finalBondPrice;
            }
            double allowedRisk;
            if (!double.TryParse(textBox2.Text, out allowedRisk))
            {
                MessageBox.Show("Please enter your allowed risk in $ (e.g. 10000).", "Require Allowed Risk");
                throw new InvalidOperationException();
            }
            IRecommendationEngine rec = tester.RiskEngine;
            assetPrices.TryGetValue(asset, out double assetPrice);
            double ltrr = ph.LTRR[asset];
            double riskRatio = rec.CalculateRiskRatio(MoneySupply.CalculateM2AdjustedToAsset(DateTime.Now.Year - Config.StartYear, ltrr), assetPrice, ph.AssetAverageRisks[asset], ph.AverageM2toPriceRatios[asset], ph.M2toPriceAmplitude[asset], ph.AssetIncomeRate[asset]);
            assetPositions.TryGetValue(asset, out double position);
            amountToInvest = rec.FindRecommendedPosition(position, allowedRisk, riskRatio, assetCombination.Count);
            amountRisked = amountToInvest * riskRatio;
        }

        private void exportHTMLReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (assetCombination.Count <= 0)
            {
                MessageBox.Show("Please select a combination of assets.", "Selection Required");
                return;
            }
            if (portfolio == null)
            {
                MessageBox.Show("Please click Calculate.", "Need to Calculate");
                return;
            }
            //determine ideal prices
            var assetIdealPrices = new Dictionary<string, double>();
            foreach (var asset in assetCombination)
            {
                AssetUtils.PredictAssetPrice(tester, Config.StartYear, asset, out double highRisk, out double medRisk, out double lowRisk);

                assetIdealPrices[asset] = medRisk / Constants.MarginOfSafety;
            }
            tester.CreateReport(ReportType.HTML, portfolio, assetPrices, assetCombination, textBox2.Text, textBox1.Text, assetPositions, assetIdealPrices);
            Process.Start("Report.html");
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start("Readme.txt");
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Process.Start("License.txt");
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            Process.Start("https://finance.yahoo.com/");
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.kitco.com/charts/livegold.html");
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            Process.Start("Help\\Help.html");
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            if (portfolioPlanner != null && portfolioPlanner.Input != null)
            {
                planInput = portfolioPlanner.Input;
            }
            else if (planInput == null)
            {
                planInput = new PlanInput();
            }
            if (portfolioPlanner == null || portfolioPlanner.IsDisposed)
            {
                portfolioPlanner = new PortfolioPlanner();
            }
            portfolioPlanner.Show();
            portfolioPlanner.Focus();
            portfolioPlanner.FormClosing += PortfolioPlanner_FormClosing;
            portfolioPlanner.Input = planInput;
        }

        private void PortfolioPlanner_FormClosing(object sender, FormClosingEventArgs e)
        {
            planInput = portfolioPlanner.Input;
        }

        private void exportCSVReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (assetCombination.Count <= 0)
            {
                MessageBox.Show("Please select a combination of assets.", "Selection Required");
                return;
            }
            if (portfolio == null)
            {
                MessageBox.Show("Please click Calculate.", "Need to Calculate");
                return;
            }
            tester.CreateReport(ReportType.CSV, portfolio, assetPrices, assetCombination, textBox2.Text, textBox1.Text, assetPositions);
            Process.Start("Report.csv");
        }
    }
}
