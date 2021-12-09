using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using server.Simulation;

namespace server.Helpers
{
    public class PerformanceSample
    {
        public readonly string Name;
        public double Min;
        public double Max;
        public double Total
        {
            get
            {
                var sum = 0d;
                for (var i = 0; i < Samples.Count; i++)
                    sum += Samples[i];
                return sum;
            }
        }
        public double Average => Total / Samples.Count;

        public readonly List<double> Samples;

        public PerformanceSample(string name)
        {
            Name = name;
            Samples = new();
        }
    }
    public static class PerformanceMetrics
    {
        static readonly Dictionary<string, PerformanceSample> SystemTimes = new();
        static readonly Dictionary<string, PerformanceSample> SystemTimesLastPeriod = new();

        public static void RegisterSystem(string systemName)
        {
            SystemTimesLastPeriod.Add(systemName, new PerformanceSample(systemName));
            SystemTimes.Add(systemName, new PerformanceSample(systemName));
        }
        public static void AddSample(string systemName, double time)
        {
            SystemTimes[systemName].Samples.Add(time);
            if (SystemTimes[systemName].Min > time)
                SystemTimes[systemName].Min = time;

            if (SystemTimes[systemName].Max < time)
                SystemTimes[systemName].Max = time;
        }

        public static void Restart()
        {
            foreach (var (name, sample) in SystemTimes)
            {
                if (sample.Samples.Count == 0)
                    continue;

                SystemTimesLastPeriod[name].Samples.Clear();
                SystemTimesLastPeriod[name].Samples.AddRange(sample.Samples);
                SystemTimesLastPeriod[name].Min = sample.Min;
                SystemTimesLastPeriod[name].Max = sample.Max;
                sample.Samples.Clear();
                sample.Min = double.MaxValue;
                sample.Max = double.MinValue;
            }
        }

        private static readonly StringBuilder Sb = new();
        public static string Draw()
        {
            Sb.Clear();
            var total = 0d;
            Sb.AppendLine($"{"Name",-20}  {"Avg",8}  {"Min",8}  {"Max",8}   {"Total",8}");
            foreach (var (name, samples) in Enumerable.OrderBy(SystemTimesLastPeriod, k => k.Value.Name))
            {
                if (name == nameof(Game))
                {
                    total = samples.Average;
                    continue;
                }
                Sb.AppendLine($"{name,-20}     {samples.Average:00.00}     {samples.Min:00.00}     {samples.Max:00.00}    {samples.Total:00.00}ms");
            }
            Sb.AppendLine($"Average Total Tick Time: {total:00.00}/{1000f / Game.TargetTps:00.00}ms ({100 * total / (1000f / Game.TargetTps):00.00}% of budget)");
            Console.SetCursorPosition(0, 0);
            var str = Sb.ToString();
            Console.Write(str);
            return str;
        }
    }
}