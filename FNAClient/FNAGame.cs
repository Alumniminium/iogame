using System;
using System.Diagnostics;
using System.Runtime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RG351MP.Managers;
using RG351MP.Scenes;
using server.Helpers;

namespace RG351MP
{
    public class GameEntry : Game
    {
        public static uint CurrentTick { get; private set; }

        public float totalTime;
        public int totalFrames;

        public static GraphicsDeviceManager DevMngr;
        public static SpriteBatch Batch;
        public static Scene Scene;
        public static Stopwatch sw;
        public static BatteryMonitor BatteryMonitor;

        [STAThread]
        static void Main() => new GameEntry() { IsFixedTimeStep = true }.Run();
        public GameEntry()
        {
            BatteryMonitor = new BatteryMonitor();
            BatteryMonitor.Start();
            // PerformanceMetrics.RegisterSystem(nameof(GameEntry));
            sw = Stopwatch.StartNew();
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            MyContentManager.Initialize(Content);
            DevMngr = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = Environment.UserName == "root" ? 640 : 640 * 2,
                PreferredBackBufferHeight = Environment.UserName == "root" ? 480 : 480 * 2,
                SynchronizeWithVerticalRetrace = false,
                GraphicsProfile = GraphicsProfile.HiDef,
                PreferMultiSampling = false,
                IsFullScreen = Environment.UserName == "root"
            };
            DevMngr.ApplyChanges();
            DevMngr.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        }

        protected override void Initialize()
        {
            Batch = new SpriteBatch(GraphicsDevice);
            base.Initialize();
            Scene = new Scenes.GameScene();
            Scene.Activate();
        }
        protected override void LoadContent()
        {
            MyContentManager.Load();
            base.LoadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            double updateStart = sw.Elapsed.TotalMilliseconds;
            var last = updateStart;
            GamepadManager.Update();
            if (!GamepadManager.CurrentState.IsConnected)
                KeyboardMouseManager.Update();
            Scene.Update(gameTime);

            if (totalTime > 1)
            {
                // PerformanceMetrics.Restart();
                // // var lines = PerformanceMetrics.Draw();
                // // FConsole.WriteLine(lines);

                // PerformanceMetrics.TicksPerSecond = 0;

                // PerformanceMetrics.FPS = totalFrames;
                totalFrames = 0;
                totalTime = 0;
            }

            CurrentTick++;
            // PerformanceMetrics.AddSample(nameof(GameEntry), sw.Elapsed.TotalMilliseconds - updateStart);
            // PerformanceMetrics.TicksPerSecond++;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            double frameStart = sw.Elapsed.TotalMilliseconds;
            DevMngr.GraphicsDevice.Clear(Color.Gray);

            Scene.Draw();

            Batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            MyContentManager.Font.DrawText(Batch, 5, 5, $"FPS: , ms, BAT: {BatteryMonitor.Percent}% {BatteryMonitor.Volts:0.00}V Power: {BatteryMonitor.PowerFlowMilliAmps}mA", Color.Red, scale: 0.4f);
            Batch.End();
            base.Draw(gameTime);
            double frameEnd = sw.Elapsed.TotalMilliseconds;

            // PerformanceMetrics.FrameTime = frameEnd - frameStart;
            totalTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            totalFrames++;
        }
    }
}