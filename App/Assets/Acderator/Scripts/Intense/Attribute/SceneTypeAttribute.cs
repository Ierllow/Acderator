using System;

namespace Intense.Attribute
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class SceneTypeAttribute : System.Attribute
    {
        public SceneType Type { get; }

        public SceneTypeAttribute(SceneType type) => Type = type;
    }
}