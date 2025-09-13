using System.Collections.Generic;
using System.Linq;
using server.Helpers;
using server.Simulation.Net;

namespace server.Simulation
{
    public static class LeaderBoard
    {
        public class LeaderBoardEntry
        {
            public string Name;
            public int Score;
        }
        private static List<LeaderBoardEntry> Entries = new();
        public static void Add(LeaderBoardEntry entry)
        {
            var foundEntry = Entries.FirstOrDefault(x => x.Name == entry.Name);
            if (foundEntry == null)
                Entries.Add(entry);
            else
                foundEntry.Score += entry.Score;
            Entries = Entries.OrderByDescending(x => x.Score).ToList();
            Broadcast();
        }
        public static void Broadcast()
        {
            for (int i = 0; i < 5; i++)
            {
                if (i < Entries.Count)
                {
                    var e = Entries[i];
                    Game.Broadcast(ChatPacket.Create(0, $"#{i + 1} {e.Name,-16} - {e.Score.FormatKMB()}", 10));
                }
                else
                    Game.Broadcast(ChatPacket.Create(0, $"#{i + 1} -  -  -", 10));
            }
        }
        public static void Remove(LeaderBoardEntry entry) => Entries.Remove(entry);
        public static void Clear() => Entries.Clear();
    }
}