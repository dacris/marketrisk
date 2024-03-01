using System.Diagnostics;

namespace MarketRisk.GUI
{
    internal class ProcessShellex
    {
        public static void Start(string path)
        {
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
        }
    }
}
