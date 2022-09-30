using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RG351MP.Managers;
using System;

namespace RG351MP.Scenes
{
    public class InputDeveloperScene : Scene
    {
        public float vibrationAmount = 0f;
        public override void Activate()
        {
        }

        public override void Destroy()
        {

        }

        public override void Draw()
        {
            GameEntry.Batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            MyContentManager.Font.DrawText(GameEntry.Batch, 10, 30, "Press START + SELECT to QUIT", Color.Red, scale: 0.5f);
            MyContentManager.Font.DrawText(GameEntry.Batch, 10, 60, "Press A/B to in/decrease Vibration: "+MathF.Round(vibrationAmount,3), Color.Red, scale: 0.5f);

            MyContentManager.Font.DrawText(GameEntry.Batch, 100, 375, "UP:     ", GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadUp) ? Color.Blue : Color.Red, scale: 0.3f);
            MyContentManager.Font.DrawText(GameEntry.Batch, 100, 425, "DOWN:   ", GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadDown) ? Color.Blue : Color.Red, scale: 0.3f);
            MyContentManager.Font.DrawText(GameEntry.Batch, 75, 400, "LEFT:    ", GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadLeft) ? Color.Blue : Color.Red, scale: 0.3f);
            MyContentManager.Font.DrawText(GameEntry.Batch, 125, 400, "RIGHT:  ", GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadRight) ? Color.Blue : Color.Red, scale: 0.3f);
            MyContentManager.Font.DrawText(GameEntry.Batch, 400, 425, "A:      ", GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A) ? Color.Blue : Color.Red, scale: 0.3f);
            MyContentManager.Font.DrawText(GameEntry.Batch, 425, 400, "B:      ", GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.B) ? Color.Blue : Color.Red, scale: 0.3f);
            MyContentManager.Font.DrawText(GameEntry.Batch, 375, 400, "X:      ", GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.X) ? Color.Blue : Color.Red, scale: 0.3f);
            MyContentManager.Font.DrawText(GameEntry.Batch, 400, 375, "Y:      ", GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Y) ? Color.Blue : Color.Red, scale: 0.3f);
            MyContentManager.Font.DrawText(GameEntry.Batch, 100, 350, "L1:     ", GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftShoulder) ? Color.Blue : Color.Red, scale: 0.3f);
            MyContentManager.Font.DrawText(GameEntry.Batch, 400, 350, "R1:     ", GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.RightShoulder) ? Color.Blue : Color.Red, scale: 0.3f);
            MyContentManager.Font.DrawText(GameEntry.Batch, 100, 325, "L2:     ", GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftTrigger) ? Color.Blue : Color.Red, scale: 0.3f);
            MyContentManager.Font.DrawText(GameEntry.Batch, 400, 325, "R2:     ", GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.RightTrigger) ? Color.Blue : Color.Red, scale: 0.3f);
            MyContentManager.Font.DrawText(GameEntry.Batch, 100, 300, "L3:     ", GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftStick) ? Color.Blue : Color.Red, scale: 0.3f);
            MyContentManager.Font.DrawText(GameEntry.Batch, 400, 300, "R3:     ", GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.RightStick) ? Color.Blue : Color.Red, scale: 0.3f);
            MyContentManager.Font.DrawText(GameEntry.Batch, 300, 425, "START:  ", GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start) ? Color.Blue : Color.Red, scale: 0.3f);
            MyContentManager.Font.DrawText(GameEntry.Batch, 200, 425, "SELECT: ", GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Back) ? Color.Blue : Color.Red, scale: 0.3f);

            var left = GamePad.GetState(PlayerIndex.One).ThumbSticks.Left;
            var v2Left = new System.Numerics.Vector2(left.X, left.Y);
            var leftStick = v2Left.ToRadians();
            MyContentManager.Font.DrawText(GameEntry.Batch, 250, 260, "" + leftStick, Color.Red, scale: 0.33f);
            GameEntry.Batch.Draw(MyContentManager.Pixel, new Rectangle(250, 150, 100, 100), new Rectangle(0, 0,1,1), Color.Green, leftStick, Vector2.Zero/*new Vector2(50, 50)*/, SpriteEffects.None, 0f);

            var right = GamePad.GetState(PlayerIndex.One).ThumbSticks.Right;
            var v2Right = new System.Numerics.Vector2(right.X, right.Y);
            var rightStick = v2Right.ToRadians();
            MyContentManager.Font.DrawText(GameEntry.Batch, 400, 260, "" + rightStick, Color.Red, scale: 0.33f);
            GameEntry.Batch.Draw(MyContentManager.Pixel, new Rectangle(400, 150, 100, 100), new Rectangle(0, 0, 1,1), Color.Blue, rightStick, Vector2.Zero/*new Vector2(50, 50)*/, SpriteEffects.None, 0f);

            if (GamePad.GetState(0).Buttons.A == ButtonState.Pressed)
            {
                vibrationAmount = MathHelper.Clamp(vibrationAmount + 0.01f, 0.0f, 1.0f);
                GamePad.SetVibration(PlayerIndex.One, vibrationAmount, vibrationAmount);
            }
            if (GamePad.GetState(0).Buttons.B == ButtonState.Pressed)
            {
                vibrationAmount = MathHelper.Clamp(vibrationAmount - 0.01f, 0.0f, 1.0f);
                GamePad.SetVibration(PlayerIndex.One, vibrationAmount, vibrationAmount);
            }

            GameEntry.Batch.End();
        }

        public override void Update(GameTime gameTime)
        {

        }
    }
}