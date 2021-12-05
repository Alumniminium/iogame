using System.Text.Json;
using iogame.Util;

namespace iogame.Simulation.Database
{
    public static class Db
    {
        public static Dictionary<int, BaseResource> BaseResources = new();
        public static void CreateResources()
        {
            var tri = new BaseResource(sides: 3, size: 10, color: Convert.ToUInt32("80ED99", 16), borderColor: 0, mass: (float)Math.Pow(10, 3), elasticity: 1.0f, drag: 0.01f, health: 200, bodyDamage: 0, maxAliveNum: 500);
            var squ = new BaseResource(sides: 4, size: 5, color: Convert.ToUInt32("DB5461", 16), borderColor: 0, mass: (float)Math.Pow(5, 3), elasticity: 1.0f, drag: 0.01f, health: 100, bodyDamage: 0, maxAliveNum: 300);
            var pen = new BaseResource(sides: 5, size: 40, color: Convert.ToUInt32("6F2DBD", 16), borderColor: 0, mass: (float)Math.Pow(20, 3), elasticity: 1.0f, drag: 0.01f, health: 400, bodyDamage: 0, maxAliveNum: 100);
            var hex = new BaseResource(sides: 6, size: 80, color: Convert.ToUInt32("FAA916", 16), borderColor: 0, mass: (float)Math.Pow(40, 3), elasticity: 1.0f, drag: 0.01f, health: 800, bodyDamage: 0, maxAliveNum: 10);
            //var oct = new BaseResource(sides: 8, size: 50, color: 0, borderColor: 0, mass: (float)Math.Pow(50, 2), elasticity: 1.0f, drag: 0.01f, health: 1000, bodyDamage: 0, maxAliveNum: 2);

            BaseResources.Add(tri.Sides, tri);
            BaseResources.Add(squ.Sides, squ);
            BaseResources.Add(pen.Sides, pen);
            BaseResources.Add(hex.Sides, hex);
            // BaseResources.Add(oct.Sides, oct);

            var serializerOptions = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
            var json = JsonSerializer.Serialize(BaseResources, serializerOptions);
            File.WriteAllText("BaseResources.json", json);
        }
        public static void LoadBaseResources()
        {
            var json = File.ReadAllText("BaseResources.json");
            BaseResources = JsonSerializer.Deserialize<Dictionary<int, BaseResource>>(json) ?? new Dictionary<int, BaseResource>();
            FConsole.WriteLine($"Loaded {BaseResources.Count} Base Resources");
        }
    }
}