using System;
using System.Collections.Generic;

namespace server.Simulation.Database
{
    public static class Db
    {
        public static Dictionary<int, BaseResource> BaseResources { get; set; } = new();
        public static void CreateResources()
        {
            var tri = new BaseResource(sides: 3, size: 4, color: Convert.ToUInt32("80ED99", 16), borderColor: 0, mass: MathF.Pow(5, 3), elasticity: 1.01f, drag: 0.01f, health: 100, bodyDamage: 0, maxAliveNum: 1000);
            var squ = new BaseResource(sides: 4, size: 8, color: Convert.ToUInt32("DB5461", 16), borderColor: 0, mass: MathF.Pow(1, 3), elasticity: 1.0f, drag: 0.01f, health: 200, bodyDamage: 0, maxAliveNum: 2000);
            var pen = new BaseResource(sides: 5, size: 12, color: Convert.ToUInt32("6F2DBD", 16), borderColor: 0, mass: MathF.Pow(10, 3), elasticity: 1f, drag: 0.01f, health: 300, bodyDamage: 0, maxAliveNum: 250);
            var hex = new BaseResource(sides: 6, size: 16, color: Convert.ToUInt32("FAA916", 16), borderColor: 0, mass: MathF.Pow(30, 3), elasticity: 11f, drag: 0.01f, health: 500, bodyDamage: 0, maxAliveNum: 20);
            var idk = new BaseResource(sides: 7, size: 20, color: Convert.ToUInt32("523E3D", 16), borderColor: 0, mass: MathF.Pow(100, 3), elasticity: 1f, drag: 0.01f, health: 2000, bodyDamage: 0, maxAliveNum: 10);
            var oct = new BaseResource(sides: 8, size: 24, color: 0, borderColor: 0, mass: MathF.Pow(50, 2), elasticity: 1.0f, drag: 1f, health: 1000, bodyDamage: 0, maxAliveNum: 1);

            BaseResources.Add(tri.Sides, tri);
            BaseResources.Add(squ.Sides, squ);
            BaseResources.Add(pen.Sides, pen);
            BaseResources.Add(hex.Sides, hex);
            BaseResources.Add(idk.Sides, idk);
            BaseResources.Add(oct.Sides, oct);

            // var serializerOptions = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
            // var json = JsonSerializer.Serialize(BaseResources, serializerOptions);
            // File.WriteAllText("BaseResources.json", json);
        }
        public static void LoadBaseResources()
        {
            // var json = File.ReadAllText("BaseResources.json");
            // BaseResources = JsonSerializer.Deserialize<Dictionary<int, BaseResource>>(json) ?? new Dictionary<int, BaseResource>();
            // FConsole.WriteLine($"Loaded {BaseResources.Count} Base Resources");
        }
    }
}