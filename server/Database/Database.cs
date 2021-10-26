using System.Text.Json;
using iogame.Util;

namespace iogame.Simulation.Database
{
    public static class Db
    {
        public static Dictionary<int, BaseResource> BaseResources = new();
        public static void CreateResources()
        {
            var triangle = new BaseResource(3, 200, 0, 0, (float)Math.Pow(200, 3), 200, 1);
            var square = new BaseResource(4, 100, 0, 0, (float)Math.Pow(100, 3), 100, 1);
            var pentagon = new BaseResource(5, 300, 0, 0, (float)Math.Pow(300, 3), 400, 1);
            var hexagon = new BaseResource(6, 400, 0, 0, (float)Math.Pow(400, 3), 800, 1);
            var octagon = new BaseResource(8, 500, 0, 0, (float)Math.Pow(500, 3), 1000, 1);

            BaseResources.Add(triangle.Sides, triangle);
            BaseResources.Add(square.Sides, square);
            BaseResources.Add(pentagon.Sides, pentagon);
            BaseResources.Add(hexagon.Sides, hexagon);
            BaseResources.Add(octagon.Sides, octagon);

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