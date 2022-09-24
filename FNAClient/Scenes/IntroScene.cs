using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using RG351MP.Managers;

namespace RG351MP.Scenes
{
    public class IntroScene : Scene
    {
        private Video video;
        private VideoPlayer videoPlayer;
        private Texture2D _videoFrame;
        private Vector2 _framePosition;

        public override void Activate()
        {
            video = MyContentManager.Video;
            videoPlayer = new VideoPlayer { IsLooped = false };
            videoPlayer.Play(video);
        }

        public override void Update(GameTime gameTime)
        {
            _videoFrame = videoPlayer.GetTexture();
            _framePosition = new Vector2((GameEntry.DevMngr.PreferredBackBufferWidth - videoPlayer.Video.Width) / 2, (GameEntry.DevMngr.PreferredBackBufferHeight - videoPlayer.Video.Height) / 2);
            if (videoPlayer.State == MediaState.Stopped || GamepadManager.AnyButtonPressed() || Keyboard.GetState().IsKeyDown(Keys.Escape))
                GoToMenu();
        }

        private void GoToMenu()
        {
            Destroy();
            GameEntry.Scene = new GameScene();
            GameEntry.Scene.Activate();
        }

        public override void Draw()
        {
            GameEntry.Batch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
            GameEntry.Batch.Draw(_videoFrame, _framePosition, Color.White);
            GameEntry.Batch.End();
        }

        public override void Destroy()
        {
            videoPlayer.Dispose();
            video = null;
        }
    }
}