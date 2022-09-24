using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RG351MP.Managers;
using RG351MP.Simulation.Net;

namespace RG351MP.Scenes
{
    public class GameScene : Scene
    {
        public static uint CurrentTick { get; private set; }
        public static Vector2 MapSize = new(1_500, 20_000);
        public static Dictionary<int, Entity> Entities = new();
        internal static readonly int TargetTps = 60;
        public static Player Player;
        public SpringCamera Camera;
        private Texture2D _background;
        private BasicEffect shader;

        public override void Activate()
        {
            shader = new(GameEntry.DevMngr.GraphicsDevice)
            {
                VertexColorEnabled = true,
                Projection = Matrix.CreateOrthographicOffCenter(0, GameEntry.DevMngr.GraphicsDevice.Viewport.Width, GameEntry.DevMngr.GraphicsDevice.Viewport.Height, 0, 0, 1)
            };
            Camera = new SpringCamera(new Viewport(0, 0, GameEntry.DevMngr.GraphicsDevice.Viewport.Width, GameEntry.DevMngr.GraphicsDevice.Viewport.Height));
            _background = MyContentManager.Space;

            // NetClient.Connect("localhost");
            NetClient.Connect("io.her.st");
        }

        public override void Update(GameTime gameTime)
        {
            Camera.Update(0.166f, 0F, Player.Position);
            GameEntry.DevMngr.GraphicsDevice.Viewport = Camera.Viewport;
        }

        public override void Draw()
        {
            GameEntry.DevMngr.GraphicsDevice.Viewport = Camera.Viewport;

            GameEntry.Batch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Camera.Transform * Matrix.CreateScale(0.1f));
            GameEntry.Batch.Draw(_background, Vector2.Zero, new Rectangle(0, 0, (int)MapSize.X, (int)MapSize.Y), Color.White, 0, Vector2.Zero, 100f, SpriteEffects.None, 1f);
            GameEntry.Batch.End();

            if (!NetClient.LoggedIn)
                return;

            foreach(var entity in Entities.Values)
            {
                if (!entity.Polygon.Initialized)
                {
                    entity.Polygon.Buffer = new VertexBuffer(GameEntry.DevMngr.GraphicsDevice, typeof(VertexPositionColor), entity.Polygon.vertexPositionColors.Length, BufferUsage.WriteOnly);
                    entity.Polygon.Buffer.SetData(entity.Polygon.vertexPositionColors);
                    entity.Polygon.Initialized = true;
                }

                var matrix = Matrix.CreateRotationZ(entity.Rotation) * Matrix.CreateTranslation(entity.Position.X, entity.Position.Y, 0);

                shader.World = matrix;
                shader.View = Camera.Transform;
                GameEntry.DevMngr.GraphicsDevice.SetVertexBuffer(entity.Polygon.Buffer);

                shader.CurrentTechnique.Passes[0].Apply();
                GameEntry.DevMngr.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, entity.Polygon.Buffer.VertexCount / 3);
            }
        }

        public override void Destroy()
        {

        }
    }
}