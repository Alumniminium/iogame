using iogame.ECS;

namespace iogame.Simulation.Managers
{
    public static class Pattern<T> where T : struct
    {
        public static bool Match(Entity entity) => entity.Has<T>();
    }
    public static class Pattern<T, T2> where T : struct where T2 : struct
    {
        public static bool Match(Entity entity) => entity.Has<T, T2>();
    }
    public static class Pattern<T, T2, T3> where T : struct where T2 : struct where T3 : struct
    {
        public static bool Match(Entity entity) => entity.Has<T, T2, T3>();
    }
    public static class Pattern<T, T2, T3, T4> where T : struct where T2 : struct where T3 : struct where T4 : struct
    {
        public static bool Match(Entity entity) => entity.Has<T, T2, T3, T4>();
    }
}