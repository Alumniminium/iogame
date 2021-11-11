using System.Numerics;
using iogame.ECS;
using iogame.Simulation.Components;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{

//                     ____
//                  _.' :  `._
//              .-.'`.  ;   .'`.-.
//     __      / : ___\ ;  /___ ; \      __
//   ,'_ ""--.:__;".-.";: :".-.":__;.--"" _`,
//   :' `.t""--.. '<@.`;_  ',@>` ..--""j.' `;
//        `:-.._J '-.-'L__ `-- ' L_..-;'
//          "-.__ ;  .-"  "-.  : __.-"
//              L ' /.------.\ ' J
//               "-.   "--"   .-"
//              __.l"-:_JL_;-";.__
//           .-j/'.;  ;""""  / .'\"-.
//         .' /:`. "-.:     .-" .';  `.
//      .-"  / ;  "-. "-..-" .-"  :    "-.
//   .+"-.  : :      "-.__.-"      ;-._   \
//   ; \  `.; ;                    : : "+. ;
//   :  ;   ; ;                    : ;  : \:
//  : `."-; ;  ;                  :  ;   ,/;
//   ;    -: ;  :                ;  : .-"'  :
//   :\     \  : ;             : \.-"      :
//    ;`.    \  ; :            ;.'_..--  / ;
//    :  "-.  "-:  ;          :/."      .'  :
//      \       .-`.\        /t-""  ":-+.   :
//       `.  .-"    `l    __/ /`. :  ; ; \  ;
//         \   .-" .-"-.-"  .' .'j \  /   ;/
//          \ / .-"   /.     .'.' ;_:'    ;
//           :-""-.`./-.'     /    `.___.'
//                 \ `t  ._  /  bug :F_P:
//                  "-.t-._:'
    public class ForceSystem : PixelSystem<PositionComponent, VelocityComponent, PhysicsComponent>
    {
        public struct ComponentPair
        {
            public Entity Entity;
            public PositionComponent PositionComponent;
            public VelocityComponent VelocityComponent;
            public PhysicsComponent PhysicsComponent;
        }

        public ComponentPair[] Data;
        public bool dirty = true;
        public ForceSystem()
        {
            Name = "Move System";

            PerformanceMetrics.RegisterSystem(Name);
        }

        public override void Update(float deltaTime, List<Entity> Entities)
        {
            if (dirty)
            {
                Data = new ComponentPair[Entities.Count];

                for (int i = 0; i < Entities.Count; i++)
                {
                    var entity = Entities[i];
                    Data[i].Entity = entity;
                    Data[i].PositionComponent = entity.Get<PositionComponent>();;
                    Data[i].PhysicsComponent = entity.Get<PhysicsComponent>();;
                    Data[i].VelocityComponent = entity.Get<VelocityComponent>();;
                }
                dirty = false;
                FConsole.WriteLine("Movement System Dirty");
            }
            for (int i = 0; i < Data.Length; i++)
            {
                var datum = Data[i];
                ref var entity = ref datum.Entity;
                ref var pos = ref datum.PositionComponent;
                ref var vel = ref datum.VelocityComponent;
                ref var phy = ref datum.PhysicsComponent;

                vel.Force *= 1f - (phy.Drag * deltaTime);

                if (vel.Force.Magnitude() < 5)
                    vel.Force = Vector2.Zero;

                pos.LastPosition = pos.Position;
                pos.Position += vel.Force * deltaTime;
                pos.Position = Vector2.Clamp(pos.Position, Vector2.Zero, new Vector2(Game.MAP_WIDTH, Game.MAP_HEIGHT));

                entity.Replace(pos);
                entity.Replace(vel);
                entity.Replace(phy);
            }
        }
        public override bool MatchesFilter(ref Entity entity)
        {
            var match = base.MatchesFilter(ref entity);
            if (match)
                dirty = true;
            return match;
        }
    }
}