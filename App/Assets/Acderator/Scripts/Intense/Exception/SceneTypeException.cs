using Cysharp.Text;
using System;

namespace Intense
{
    internal class InvalidSceneTypeException : Exception
    {
        internal InvalidSceneTypeException(ESceneType sceneType)
            : base(ZString.Format("The {0} is invalid scene type.", sceneType.ToString())) { }
    }

    internal class DuplicateSceneTypeException : Exception
    {
        internal DuplicateSceneTypeException(ESceneType sceneType)
            : base(ZString.Format("The {0} is duplicate scene type.", sceneType.ToString())) { }
    }
}