using System.Collections.Generic;
using System.Linq;
using Packets;

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

            for (int i = 0; i < 5; i++)
            {
                if (i < Entries.Count)
                {
                    var e = Entries[i];
                    // format score to 1k 10k 100k
                    var score = e.Score;
                    var suffix = "";
                    if (score >= 1000000)
                    {
                        score /= 1000000;
                        suffix = "M";
                    }
                    else if (score >= 1000)
                    {
                        score /= 1000;
                        suffix = "K";
                    }
                    Game.Broadcast(ChatPacket.Create(0, $"#{i + 1} {e.Name} - {score}{suffix}", 10));
                }
                else
                    Game.Broadcast(ChatPacket.Create(0, $"#{i + 1} -  -  -", 10));
            }
        }
        public static void Remove(LeaderBoardEntry entry) => Entries.Remove(entry);
        public static void Clear() => Entries.Clear();
    }
}