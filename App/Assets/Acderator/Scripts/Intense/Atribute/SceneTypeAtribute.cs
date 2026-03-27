using System;

namespace Intense.Attribute
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class SceneTypeAttribute : System.Attribute
    {
        public ESceneType Type { get; }

        public SceneTypeAttribute(ESceneType type) => Type = type;
    }
}