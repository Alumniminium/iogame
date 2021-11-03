using System;
using System.Text;
using iogame.Simulation;

namespace iogame.Util
{
    public class PerformanceSample
    {
        public string Name;
        public double Min;
        public double Max;
        public double Average;

        public List<double> Samples;

        public PerformanceSample(string name)
        {
            Name = name;
            Samples = new();
        }
    }
    public static class PerformanceMetrics
    {
        public static Dictionary<string, PerformanceSample> SystemTimes = new();
        public static Dictionary<string, PerformanceSample> SystemTimesLastPeriod = new();

        public static void RegisterSystem(string systemName)
        {
            SystemTimesLastPeriod.Add(systemName, new PerformanceSample(systemName));
            SystemTimes.Add(systemName, new PerformanceSample(systemName));
        }
        public static void AddSample(string systemName, double time) => SystemTimes[systemName].Samples.Add(time);

        public static void Restart()
        {
            foreach (var kvp in SystemTimes)
            {
                if (kvp.Value.Samples.Count == 0)
                    continue;
                SystemTimesLastPeriod[kvp.Key].Min = kvp.Value.Samples.Min();
                SystemTimesLastPeriod[kvp.Key].Max = kvp.Value.Samples.Max();
                SystemTimesLastPeriod[kvp.Key].Average = kvp.Value.Samples.Average();
                SystemTimesLastPeriod[kvp.Key].Samples.Clear();
                SystemTimesLastPeriod[kvp.Key].Samples.AddRange(kvp.Value.Samples);
                kvp.Value.Samples.Clear();
            }
        }

        public static void Draw()
        {
            var sb = new StringBuilder();
            var total = 0d;
            sb.AppendLine($"{"Name",-24} {"Avg",8} {"Min",8} {"Max",8}");
            foreach (var kvp in SystemTimesLastPeriod)
            {
                total += kvp.Value.Average;
                sb.AppendLine($"{kvp.Key,-24}      {kvp.Value.Average:0.00}    {kvp.Value.Min:0.00}    {kvp.Value.Max:0.00} ms");
            }
            var gen0 = GC.CollectionCount(0);
            var gen1 = GC.CollectionCount(1);
            var gen2 = GC.CollectionCount(2);
            sb.AppendLine($"GC 0-2: {gen0}, {gen1}, {gen2}");
            sb.AppendLine($"Average Total Tick Time: {total:0.0}/{1000f / Game.TARGET_TPS:0.0}ms ({100 * total / (1000f/Game.TARGET_TPS):0.0}% of budget)");
            Console.SetCursorPosition(0,0);
            Console.Write(sb);
        }
    }
}