using System.Numerics;
using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct DirectionalEngineComponent
{
    public float Thrust;
    public float PowerDraw;
    // Thrust direction determined by GridPositionComponent.Rotation
}

[Component]
public struct WeaponMountComponent
{
    public WeaponType Type;
    public float Damage;
    public float FireRate;
    public float Range;
    public float LastFireTime;
    // Fire direction determined by GridPositionComponent.Rotation

    public readonly bool CanFire() => NttWorld.Tick - LastFireTime > (60f / FireRate);
}

[Component]
public struct DirectionalShieldComponent
{
    public float MaxShield;
    public float CurrentShield;
    public float RegenRate;
    public float Arc;  // Coverage arc in radians (e.g., PI/2 for 90°)
    // Shield direction determined by GridPositionComponent.Rotation
}

[Component]
public struct HullComponent
{
    public float Mass;
    public float Armor;
}

[Component]
public struct AssemblyComponent
{
    public bool IsMobile;      // Can this ship move?
    public float TotalMass;    // Combined mass of all blocks
    public Vector2 CenterOfMass;
}

public enum WeaponType
{
    Laser = 0,
    Kinetic = 1,
    Plasma = 2
}