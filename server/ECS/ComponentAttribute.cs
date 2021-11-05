using System;

namespace iogame.ECS
{
    [AttributeUsage(System.AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class ComponentAttribute : Attribute { }
}