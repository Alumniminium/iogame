using System;
using Microsoft.Xna.Framework.Input;

namespace RG351MP.Managers
{
    public static class KeyboardMouseManager
    {
        public static KeyboardState CurrentState;
        public static KeyboardState LastState;
        public static MouseState CurrentMouseState;
        public static MouseState LastMouseState;


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

            // ref var inp = ref Scenes.GameScene.Player.Entity.Get<InputComponent>();
            // ref var phy = ref Scenes.GameScene.Player.Entity.Get<PhysicsComponent>();
            // ref var eng = ref Scenes.GameScene.Player.Entity.Get<EngineComponent>();
            // ref var wep = ref Scenes.GameScene.Player.Entity.Get<WeaponComponent>();

            // if(Down(Keys.Space))
            //     wep.Fire=true;
            // else
            //     wep.Fire=false;

            // if(Down(Keys.A))
            //     phy.AngularVelocity = -1;
            // else if(Down(Keys.D))
            //     phy.AngularVelocity = 1;
            // else
            //     phy.AngularVelocity = 0;
            
            // if (Down(Keys.LeftShift))
            //     eng.Throttle = 100;
            // else
            //     eng.Throttle = 0;

            // if (ButtonPressed(Keys.R))
            //     eng.RCS = !eng.RCS;

        }
    }
}