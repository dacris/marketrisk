using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarketRisk.Testing
{
    public class CSVHelper : List<string[]>
    {
        protected string csv = string.Empty;
        protected string separator = ",";

        public CSVHelper(string csv)
        {
            this.csv = csv;

            foreach (string line in csv.Split('\r', '\n').ToList().Where(s => !string.IsNullOrEmpty(s)))
            {
                string[] values = Regex.Split(line, separator);

                for (int i = 0; i < values.Length; i++)
                {
                    //Trim values
                    values[i] = values[i].Trim('\"');
                }

                this.Add(values);
            }
        }
    }
}
