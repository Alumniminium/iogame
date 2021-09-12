using System.Diagnostics;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    public class Game
    {
        public const int MAP_WIDH = 500;
        public const int MAP_HEIGHT = 500;
        public Dictionary<int, Entity> Entities = new ();
        float secondsPassed;
        float oldTimeStamp = 0;
        Thread worker;
        public void Start()
        {
            for(int i = 0; i< 100; i++)
            {
                var x = Random.Shared.Next(0,MAP_WIDH);
                var y = Random.Shared.Next(0, MAP_HEIGHT);
                var vX = Random.Shared.Next(-1,2);
                var vY = Random.Shared.Next(-1,2);
                var entity = new YellowSquare(x,y,vX,vY);

                Entities.Add(i,entity);
            }

            worker = new Thread(GameLoop);
            worker.IsBackground=true;
            worker.Start();
        }

        internal void AddPlayer(Player player)
        {
            Entities.Add(Entities.Count, player);
        }

        public async void GameLoop()
        {   
            var stopwatch = new Stopwatch();
            var fps = 30f;
            var sleepTime = 1000 / fps;
            var prevTime = DateTime.UtcNow;

            while (true)
            {
                stopwatch.Restart();
                var now = DateTime.UtcNow;
                var dt = (float)(now - prevTime).TotalSeconds;
                prevTime = now;
                var curFps = Math.Round(1 / dt);
                //Console.WriteLine(curFps);

                foreach(var kvp in Entities)
                {
                    kvp.Value.Update(dt);
                }
                Thread.Sleep(10);
                await Task.Delay(TimeSpan.FromMilliseconds(Math.Max(1, sleepTime -stopwatch.ElapsedMilliseconds))); //Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(1, 16)));
            }
        }
    }
}