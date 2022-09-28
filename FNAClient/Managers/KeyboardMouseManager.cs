using System;
using Microsoft.Xna.Framework.Input;
using RG351MP.Scenes;
using RG351MP.Simulation.Net;
using server.Simulation.Net.Packets;

namespace RG351MP.Managers
{
    public static class KeyboardMouseManager
    {
        public static KeyboardState CurrentState;
        public static KeyboardState LastState;
        public static MouseState CurrentMouseState;
        public static MouseState LastMouseState;
        public static PlayerInput LastInputs;


        internal static bool Down(Keys button) => CurrentState.IsKeyDown(button);
        internal static bool Down(Keys b1, Keys b2) => CurrentState.IsKeyDown(b1) && CurrentState.IsKeyDown(b2);

        public static bool ButtonPressed(Keys button) => LastState.IsKeyDown(button) && CurrentState.IsKeyUp(button);
        public static bool ButtonsPressed(Keys b1, Keys b2)
        {
            if (LastState.IsKeyDown(b1) && CurrentState.IsKeyUp(b1))
                if (LastState.IsKeyDown(b2) && CurrentState.IsKeyUp(b2))
                    return true;

            return false;
        }
        public static void Update()
        {
            LastState = CurrentState;
            CurrentState = Keyboard.GetState();
            LastMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();

            if (ButtonPressed(Keys.Escape))
                Environment.Exit(0);

            if (Scenes.GameScene.Player == null)
                return;

            var inputs = PlayerInput.None;

            if (Down(Keys.LeftShift) || Down(Keys.RightShift))
                inputs |= PlayerInput.Boost;
            if(Down(Keys.A) || Down(Keys.Left))
                inputs |= PlayerInput.Left;
            if(Down(Keys.D) || Down(Keys.Right))
                inputs |= PlayerInput.Right;
            if(Down(Keys.Space))
                inputs |= PlayerInput.Fire;
            if(ButtonPressed(Keys.R))
                inputs |= PlayerInput.RCS;
            
            if(inputs!=LastInputs)
                NetClient.Send(PlayerMovementPacket.Create(GameScene.Player.UniqueId, 0, inputs, default));
            
            LastInputs = inputs;
        }
    }
}