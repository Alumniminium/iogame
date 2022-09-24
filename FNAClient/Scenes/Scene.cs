using Microsoft.Xna.Framework;

namespace RG351MP.Scenes
{
    public abstract class Scene
    {
        public abstract void Activate();
        public abstract void Update(GameTime gameTime);
        public abstract void Draw();
        public abstract void Destroy();
    }
}