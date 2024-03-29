using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace server.Simulation.Database
{
    public static class Db
    {
        public static Dictionary<int, BaseResource> BaseResources { get; set; } = new();
        public static void CreateResources()
        {
            var tri = new BaseResource(sides: 3, size: 3, color: Convert.ToUInt32("80ED99", 16), borderColor: 0, mass: MathF.Pow(5, 3), elasticity: 0.09f, drag: 0.01f, health: 100, bodyDamage: 0, maxAliveNum: 1000);
            var squ = new BaseResource(sides: 4, size: 4, color: Convert.ToUInt32("DB5461", 16), borderColor: 0, mass: MathF.Pow(1, 3), elasticity: 0.09f, drag: 0.01f, health: 200, bodyDamage: 0, maxAliveNum: 2000);
            var pen = new BaseResource(sides: 5, size: 8, color: Convert.ToUInt32("6F2DBD", 16), borderColor: 0, mass: MathF.Pow(10, 3), elasticity: 0.09f, drag: 0.01f, health: 300, bodyDamage: 0, maxAliveNum: 1000);
            var hex = new BaseResource(sides: 6, size: 12, color: Convert.ToUInt32("FAA916", 16), borderColor: 0, mass: MathF.Pow(30, 3), elasticity: 0.09f, drag: 0.01f, health: 500, bodyDamage: 0, maxAliveNum: 500);
            var idk = new BaseResource(sides: 7, size: 14, color: Convert.ToUInt32("523E3D", 16), borderColor: 0, mass: MathF.Pow(100, 3), elasticity: 0.09f, drag: 0.01f, health: 2000, bodyDamage: 0, maxAliveNum: 250);
            var oct = new BaseResource(sides: 8, size: 16, color: 0, borderColor: 0, mass: MathF.Pow(50, 2), elasticity: 1.0f, drag: 1f, health: 1000, bodyDamage: 0, maxAliveNum: 100);

            BaseResources.Add(tri.Sides, tri);
            BaseResources.Add(squ.Sides, squ);
            BaseResources.Add(pen.Sides, pen);
            BaseResources.Add(hex.Sides, hex);
            BaseResources.Add(idk.Sides, idk);
            BaseResources.Add(oct.Sides, oct);

            var serializerOptions = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
            var json = JsonSerializer.Serialize(BaseResources, serializerOptions);
            File.WriteAllText("BaseResources.json", json);
        }
        public static void LoadBaseResources()
        {
            // var json = File.ReadAllText("BaseResources.json");
            // BaseResources = JsonSerializer.Deserialize<Dictionary<int, BaseResource>>(json) ?? new Dictionary<int, BaseResource>();
            // FConsole.WriteLine($"Loaded {BaseResources.Count} Base Resources");
        }
    }
}