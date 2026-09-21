using System;

namespace Intense
{
    internal class InvalidSceneTypeException : Exception
    {
        internal InvalidSceneTypeException(SceneType sceneType)
            : base(string.Format("The {0} is invalid scene type.", sceneType)) { }
    }

    internal class DuplicateSceneTypeException : Exception
    {
        internal DuplicateSceneTypeException(SceneType sceneType)
            : base(string.Format("The {0} is duplicate scene type.", sceneType)) { }
    }
}