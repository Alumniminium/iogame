using System;
using System.Collections.Generic;
using System.Drawing;

namespace server.Simulation.Database;

public static class Db
{
    public static Dictionary<int, BaseResource> BaseResources { get; set; } = new();
    public static void CreateResources()
    {
        var tri = new BaseResource(sides: 3, size: 8, color: Convert.ToUInt32("80ED99", 16), borderColor: 0, elasticity: 0.09f, drag: 0.1f, health: 100, bodyDamage: 0, maxAliveNum: 2000);
        var squ = new BaseResource(sides: 4, size: 9, color: Convert.ToUInt32("DB5461", 16), borderColor: 0, elasticity: 0.09f, drag: 0.1f, health: 200, bodyDamage: 0, maxAliveNum: 4000);
        var pen = new BaseResource(sides: 5, size: 10, color: Convert.ToUInt32("6F2DBD", 16), borderColor: 0, elasticity: 0.09f, drag: 0.1f, health: 300, bodyDamage: 0, maxAliveNum: 2000);
        var hex = new BaseResource(sides: 6, size: 11, color: Convert.ToUInt32("FAA916", 16), borderColor: 0, elasticity: 0.09f, drag: 0.1f, health: 500, bodyDamage: 0, maxAliveNum: 1500);
        var idk = new BaseResource(sides: 7, size: 12, color: Convert.ToUInt32("523E3D", 16), borderColor: 0, elasticity: 0.09f, drag: 0.1f, health: 2000, bodyDamage: 0, maxAliveNum: 550);
        var oct = new BaseResource(sides: 8, size: 13, color: 0, borderColor: 0, elasticity: 1.0f, drag: 1f, health: 1000, bodyDamage: 0, maxAliveNum: 200);

        BaseResources.Add(3, tri);
        BaseResources.Add(4, squ);
        BaseResources.Add(5, pen);
        BaseResources.Add(6, hex);
        BaseResources.Add(7, idk);
        BaseResources.Add(8, oct);
    }
}