using System;
using System.IO;
using System.Threading;

namespace RG351MP
{
    public class BatteryMonitor
    {
        private Thread WorkerThread;

        public float Percent;
        public float Volts;
        public float PowerFlowMilliAmps;

        public void Start()
        {
            WorkerThread = new(WorkLoop)
            {
                IsBackground = true,
                Name = "Battery Montior"
            };
            WorkerThread.Start();
        }

        public void WorkLoop()
        {
            var path = "/sys/class/power_supply/battery/uevent";
            if (!File.Exists(path))
                path = "/sys/class/power_supply/BAT0/uevent";

            while (true)
            {
                var text = File.ReadAllText(path).Replace("POWER_SUPPLY_", "").Split(Environment.NewLine);

                for (int i = 0; i < text.Length; i++)
                {
                    var kvp = text[i].Split('=');

                    if (kvp[0] == "CURRENT_NOW")
                        PowerFlowMilliAmps = int.Parse(kvp[1]) / 1000f;
                    if (kvp[0] == "VOLTAGE_NOW")
                        Volts = int.Parse(kvp[1]) / 1_000_000f;
                    if (kvp[0] == "CAPACITY")
                        Percent = int.Parse(kvp[1]);
                }

                Thread.Sleep(1000);
            }
        }
    }
}