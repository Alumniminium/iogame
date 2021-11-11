using System;

namespace iogame.ECS
{
    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class ComponentAttribute : Attribute { }
}