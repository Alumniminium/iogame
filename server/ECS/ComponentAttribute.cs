using System;

namespace server.ECS
{
    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class ComponentAttribute : Attribute { }
}