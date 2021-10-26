using System.Text.Json;
using iogame.Util;

namespace iogame.Simulation.Database
{
    public static class Db
    {
        public static Dictionary<int, BaseResource> BaseResources = new();
        public static void CreateResources()
        {
            var tri = new BaseResource(3, 200, 0, 0, (float)Math.Pow(200, 2), 1.00f, 0.9f, 200, 1, 500);
            var squ = new BaseResource(4, 100, 0, 0, (float)Math.Pow(100, 2), 0.00f, 0.9f, 100, 1, 750);
            var pen = new BaseResource(5, 300, 0, 0, (float)Math.Pow(300, 2), -1.0f, 0.9f, 400, 1, 350);
            var hex = new BaseResource(6, 400, 0, 0, (float)Math.Pow(400, 2), 0.50f, 0.9f, 800, 1, 100);
            var oct = new BaseResource(8, 500, 0, 0, (float)Math.Pow(500, 2), -0.5f, 0.9f, 1000, 1, 10);

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