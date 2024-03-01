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
    public partial class SinglePrice : UserControl
    {
        public event EventHandler PredictPrice;
        public string Prompt { set { label1.Text = value; } }
        public string Input { get { return textBox1.Text; } set { textBox1.Text = value; } }
        public string PositionSize { get { return textBox2.Text; } set { textBox2.Text = value; } }
        public SinglePrice()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (PredictPrice != null)
            {
                PredictPrice.Invoke(sender, e);
            }
        }
    }
}
