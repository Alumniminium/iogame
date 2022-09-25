using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RG351MP.Scenes;
using RG351MP.Simulation.Net;
using server.Simulation.Net.Packets;

namespace RG351MP.Managers
{
    public static class GamepadManager
    {
        static readonly Buttons[] HardwareButtons = new[] { Buttons.A, Buttons.B, Buttons.X, Buttons.Y, Buttons.DPadDown, Buttons.DPadUp, Buttons.DPadLeft, Buttons.DPadRight, Buttons.Back, Buttons.Start, Buttons.LeftShoulder, Buttons.RightShoulder, Buttons.LeftTrigger, Buttons.RightTrigger, Buttons.LeftStick, Buttons.RightStick };
        public static GamePadState CurrentState;
        public static GamePadState LastState;
        public static PlayerInput LastInputs;

        internal static bool Down(Buttons button) => CurrentState.IsButtonDown(button);

        public static bool ButtonPressed(Buttons button) => LastState.IsButtonDown(button) && CurrentState.IsButtonUp(button);
        public static bool ButtonsPressed(Buttons button1, Buttons button2)
        {
            if (LastState.IsButtonDown(button1) && CurrentState.IsButtonUp(button1))
                if (LastState.IsButtonDown(button2) && CurrentState.IsButtonUp(button2))
                    return true;

            return false;
        }
        public static bool AnyButtonPressed()
        {
            for (int i = 0; i < HardwareButtons.Length; i++)
            {
                if (ButtonPressed(HardwareButtons[i]))
                    return true;
            }
            return false;
        }
        public static void Update()
        {
            LastState = CurrentState;
            CurrentState = GamePad.GetState(0);

            if (Down(Buttons.Start))
                Environment.Exit(0);

            if(GameScene.Player==null)
                return;

            var inputs = PlayerInput.None;

            if (Down(Buttons.A))
                inputs |= PlayerInput.Boost;
            if(CurrentState.ThumbSticks.Left.X < -0.5)
                inputs |= PlayerInput.Left;
            if(CurrentState.ThumbSticks.Left.X > 0.5)
                inputs |= PlayerInput.Right;
            if(Down(Buttons.RightShoulder))
                inputs |= PlayerInput.Fire;
            if(Down(Buttons.LeftShoulder))
                inputs |= PlayerInput.RCS;
            
            if(inputs!=LastInputs)
                NetClient.Send(PlayerMovementPacket.Create(GameScene.Player.UniqueId, 0, inputs, Vector2.Zero));
            
            LastInputs = inputs;
            // ref var inp = ref GameScene.Player.Entity.Get<InputComponent>();
            // ref readonly var phy = ref GameScene.Player.Entity.Get<PhysicsComponent>();
            // inp.Acceleration = CurrentState.ThumbSticks.Right;
            // inp.Rotation = CurrentState.ThumbSticks.Left;

            // if(ButtonPressed(Buttons.A))
            //     inp.Boost = !inp.Boost;
            // if(ButtonPressed(Buttons.DPadUp))
            //     inp.RCS = !inp.RCS;
            
            // inp.Fire = Down(Buttons.RightShoulder);
            // inp.Drop = Down(Buttons.Y);
        }
    }
}