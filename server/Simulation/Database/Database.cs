using System;
using System.Collections.Generic;

namespace server.Simulation.Database;

public static class Db
{
    public static Dictionary<int, BaseResource> BaseResources { get; set; } = new();
    public static void CreateResources()
    {
        var tri = new BaseResource(sides: 4, size: 13, color: Convert.ToUInt32("80ED99", 16), borderColor: 0, mass: MathF.Pow(5, 3), elasticity: 0.09f, drag: 0.01f, health: 100, bodyDamage: 0, maxAliveNum: 2000);
        var squ = new BaseResource(sides: 4, size: 14, color: Convert.ToUInt32("DB5461", 16), borderColor: 0, mass: MathF.Pow(1, 3), elasticity: 0.09f, drag: 0.01f, health: 200, bodyDamage: 0, maxAliveNum: 4000);
        var pen = new BaseResource(sides: 4, size: 18, color: Convert.ToUInt32("6F2DBD", 16), borderColor: 0, mass: MathF.Pow(10, 3), elasticity: 0.09f, drag: 0.01f, health: 300, bodyDamage: 0, maxAliveNum: 2000);
        var hex = new BaseResource(sides: 4, size: 22, color: Convert.ToUInt32("FAA916", 16), borderColor: 0, mass: MathF.Pow(30, 3), elasticity: 0.09f, drag: 0.01f, health: 500, bodyDamage: 0, maxAliveNum: 1500);
        var idk = new BaseResource(sides: 4, size: 24, color: Convert.ToUInt32("523E3D", 16), borderColor: 0, mass: MathF.Pow(100, 3), elasticity: 0.09f, drag: 0.01f, health: 2000, bodyDamage: 0, maxAliveNum: 550);
        var oct = new BaseResource(sides: 4, size: 26, color: 0, borderColor: 0, mass: MathF.Pow(50, 2), elasticity: 1.0f, drag: 1f, health: 1000, bodyDamage: 0, maxAliveNum: 200);

        BaseResources.Add(3, tri);
        BaseResources.Add(4, squ);
        BaseResources.Add(5, pen);
        BaseResources.Add(6, hex);
        BaseResources.Add(7, idk);
        BaseResources.Add(8, oct);
    }
}