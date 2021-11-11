using System.Text.Json;
using iogame.Util;

namespace iogame.Simulation.Database
{
    public static class Db
    {
        public static Dictionary<int, BaseResource> BaseResources = new();
        public static void CreateResources()
        {
            var tri = new BaseResource(sides: 3, size: 150, color: 0, borderColor: 0, mass: (float)Math.Pow(150, 3), elasticity: 1.0f, drag: 0.9f, health: 200, bodyDamage: 1, maxAliveNum: 3000);
            var squ = new BaseResource(sides: 4, size: 100, color: 0, borderColor: 0, mass: (float)Math.Pow(100, 3), elasticity: 1.0f, drag: 0.9f, health: 100, bodyDamage: 1, maxAliveNum: 0);
            var pen = new BaseResource(sides: 5, size: 200, color: 0, borderColor: 0, mass: (float)Math.Pow(200, 3), elasticity: 1.0f, drag: 0.9f, health: 400, bodyDamage: 1, maxAliveNum: 0);
            var hex = new BaseResource(sides: 6, size: 300, color: 0, borderColor: 0, mass: (float)Math.Pow(300, 3), elasticity: 1.0f, drag: 0.9f, health: 800, bodyDamage: 1, maxAliveNum: 0);
            var oct = new BaseResource(sides: 8, size: 500, color: 0, borderColor: 0, mass: (float)Math.Pow(500, 3), elasticity: 1.0f, drag: 0.9f, health: 1000, bodyDamage: 1, maxAliveNum: 0);

            BaseResources.Add(tri.Sides, tri);
            BaseResources.Add(squ.Sides, squ);
            BaseResources.Add(pen.Sides, pen);
            BaseResources.Add(hex.Sides, hex);
            BaseResources.Add(oct.Sides, oct);

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