using System;
using System.Collections.Generic;
using System.Drawing;

namespace server.Simulation.Database;

public static class Db
{
    public static Dictionary<int, BaseResource> BaseResources { get; set; } = new();
    public static void CreateResources()
    {
        var tri = new BaseResource(sides: 3, color: Convert.ToUInt32("80ED99", 16), borderColor: 0, elasticity: 0.09f, drag: 0.1f, health: 100, maxAliveNum: 2000);
        var squ = new BaseResource(sides: 4, color: Convert.ToUInt32("DB5461", 16), borderColor: 0, elasticity: 0.09f, drag: 0.1f, health: 200, maxAliveNum: 4000);
        var pen = new BaseResource(sides: 5, color: Convert.ToUInt32("6F2DBD", 16), borderColor: 0, elasticity: 0.09f, drag: 0.1f, health: 300, maxAliveNum: 2000);
        var hex = new BaseResource(sides: 6, color: Convert.ToUInt32("FAA916", 16), borderColor: 0, elasticity: 0.09f, drag: 0.1f, health: 500, maxAliveNum: 1500);
        var idk = new BaseResource(sides: 7, color: Convert.ToUInt32("523E3D", 16), borderColor: 0, elasticity: 0.09f, drag: 0.1f, health: 700, maxAliveNum: 1200);
        var oct = new BaseResource(sides: 8, color: Convert.ToUInt32("12313D", 16), borderColor: 0, elasticity: 1.00f, drag: 1.0f, health: 900, maxAliveNum: 1000);

        BaseResources.Add(3, tri);
        BaseResources.Add(4, squ);
        BaseResources.Add(5, pen);
        BaseResources.Add(6, hex);
        BaseResources.Add(7, idk);
        BaseResources.Add(8, oct);
    }
}