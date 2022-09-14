using System;
using System.Collections.Generic;
using System.Text;
using server.ECS;
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
        private static readonly int[] _genCollections = new int[GC.MaxGeneration];
        private static readonly int[] _genCollectionsLast = new int[GC.MaxGeneration];
        private static readonly Dictionary<string, PerformanceSample> SystemTimes = new();
        private static readonly Dictionary<string, PixelSystem> Systems = new();
        private static readonly Dictionary<string, PerformanceSample> SystemTimesLastPeriod = new();
        private static readonly StringBuilder sb = new();

        public static void RegisterSystem(PixelSystem system)
        {
            SystemTimesLastPeriod.Add(system.Name, new PerformanceSample(system.Name));
            SystemTimes.Add(system.Name, new PerformanceSample(system.Name));
            Systems.Add(system.Name, system);
        }
        public static void RegisterSystem(string systemName)
        {
            SystemTimesLastPeriod.Add(systemName, new PerformanceSample(systemName));
            SystemTimes.Add(systemName, new PerformanceSample(systemName));
            Systems.Add(systemName, null);
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
            for (var i = 0; i < GC.MaxGeneration; i++)
            {
                _genCollectionsLast[i] = _genCollections[i];
                _genCollections[i] = GC.CollectionCount(i);
            }
            sb.Clear();
        }
        public static string Draw()
        {
            var total = 0d;
            sb.AppendLine($"{"Name",-30}{"Avg",-10}{"Min",-10}{"Max",-10}{"Total",-10}{"Entities",-10}");
            foreach (var (name, samples) in SystemTimesLastPeriod)
            {
                if (name == nameof(Game))
                {
                    total = samples.Average;
                    continue;
                }
                sb.AppendLine($"{name,-30}{$"{samples.Average:#0.00}",-10}{$"{samples.Min:#0.00}",-10}{$"{samples.Max:#0.00}",-10}{$"{samples.Total:#0.00}",-10}{$"{Systems[name]?._entities.Count}",-10}");
            }
            sb.AppendLine($"Average Total Tick Time: {total:#0.00}/{1000f / Game.TargetTps:#0.00}ms ({100 * total / (1000f / Game.TargetTps):#0.00}% of budget)");

            sb.Append("GC: ");
            for (var i = 0; i < GC.MaxGeneration; i++)
                sb.Append($"Gen{i}: {_genCollections[i]}\t");
            sb.AppendLine($"Entities: {PixelWorld.EntityCount}\t Grid: {Game.Grid.EntityCount}\t");

            return sb.ToString();
        }
    }
}