using System.Text.Json;
using server.Helpers;

namespace server.Simulation.Database
{
    public static class Db
    {
        public static Dictionary<int, BaseResource> BaseResources = new();
        public static void CreateResources()
        {
            var tri = new BaseResource(sides: 3, size: 10, color: Convert.ToUInt32("80ED99", 16), borderColor: 0, mass: MathF.Pow(5, 3), elasticity: 1.0f, drag: 0.01f, health: 20, bodyDamage: 0, maxAliveNum: 500);
            var squ = new BaseResource(sides: 4, size: 5, color: Convert.ToUInt32("DB5461", 16), borderColor: 0,  mass: MathF.Pow(1, 3), elasticity: 0.5f, drag: 0.01f, health: 10, bodyDamage: 0, maxAliveNum: 15000);
            var pen = new BaseResource(sides: 5, size: 40, color: Convert.ToUInt32("6F2DBD", 16), borderColor: 0, mass: MathF.Pow(10, 3), elasticity: 1.0f, drag: 0.01f, health: 200, bodyDamage: 0, maxAliveNum: 10);
            var hex = new BaseResource(sides: 6, size: 80, color: Convert.ToUInt32("FAA916", 16), borderColor: 0, mass: MathF.Pow(30, 3), elasticity: 1.0f, drag: 0.01f, health: 500, bodyDamage: 0, maxAliveNum: 1);
            //var oct = new BaseResource(sides: 8, size: 50, color: 0, borderColor: 0, mass: MathF.Pow(50, 2), elasticity: 1.0f, drag: 0.01f, health: 1000, bodyDamage: 0, maxAliveNum: 2);

            BaseResources.Add(tri.Sides, tri);
            BaseResources.Add(squ.Sides, squ);
            BaseResources.Add(pen.Sides, pen);
            BaseResources.Add(hex.Sides, hex);
            // BaseResources.Add(oct.Sides, oct);b

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