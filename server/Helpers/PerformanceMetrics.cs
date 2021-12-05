using System;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using iogame.Simulation;
using Microsoft.AspNetCore.SignalR;

namespace iogame.Util
{
    public class PerformanceSample
    {
        public string Name;
        public double Min;
        public double Max;
        public double Total
        {
            get
            {
                var sum = 0d;
                for (int i = 0; i < Samples.Count; i++)
                    sum += Samples[i];
                return sum;
            }
        }
        public double Average
        {
            get
            {
                return Total / Samples.Count;
            }
        }

        public List<double> Samples;

        public PerformanceSample(string name)
        {
            Name = name;
            Samples = new();
        }
    }
    public static class PerformanceMetrics
    {
        static readonly Dictionary<string, PerformanceSample> _systemTimes = new();
        static readonly Dictionary<string, PerformanceSample> _systemTimesLastPeriod = new();

        public static void RegisterSystem(string systemName)
        {
            _systemTimesLastPeriod.Add(systemName, new PerformanceSample(systemName));
            _systemTimes.Add(systemName, new PerformanceSample(systemName));
        }
        public static void AddSample(string systemName, double time)
        {
            _systemTimes[systemName].Samples.Add(time);
            if (_systemTimes[systemName].Min > time)
                _systemTimes[systemName].Min = time;

            if (_systemTimes[systemName].Max < time)
                _systemTimes[systemName].Max = time;
        }

        public static void Restart()
        {
            foreach (var kvp in _systemTimes)
            {
                if (kvp.Value.Samples.Count == 0)
                    continue;

                _systemTimesLastPeriod[kvp.Key].Samples.Clear();
                _systemTimesLastPeriod[kvp.Key].Samples.AddRange(kvp.Value.Samples);
                _systemTimesLastPeriod[kvp.Key].Min = kvp.Value.Min;
                _systemTimesLastPeriod[kvp.Key].Max = kvp.Value.Max;
                kvp.Value.Samples.Clear();
                kvp.Value.Min = double.MaxValue;
                kvp.Value.Max = double.MinValue;
            }
        }

        private static readonly StringBuilder sb = new();
        public static string Draw()
        {
            sb.Clear();
            var total = 0d;
            sb.AppendLine($"{"Name",-20}  {"Avg",8}  {"Min",8}  {"Max",8}   {"Total",8}");
            foreach (var kvp in _systemTimesLastPeriod.OrderBy(k => k.Value.Name))
            {
                if (kvp.Key == nameof(Game))
                {
                    total = kvp.Value.Average;
                    continue;
                }
                sb.AppendLine($"{kvp.Key,-20}     {kvp.Value.Average:00.00}     {kvp.Value.Min:00.00}     {kvp.Value.Max:00.00}    {kvp.Value.Total:00.00}ms");
            }
            sb.AppendLine($"Average Total Tick Time: {total:00.00}/{1000f / Game.TARGET_TPS:00.00}ms ({100 * total / (1000f / Game.TARGET_TPS):00.00}% of budget)");
            Console.SetCursorPosition(0, 0);
            var str = sb.ToString();
            Console.Write(str);
            return str;
        }
    }
}