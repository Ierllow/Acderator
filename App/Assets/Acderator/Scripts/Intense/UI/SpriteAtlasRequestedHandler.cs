using System;
using UnityEngine;
using UnityEngine.U2D;

namespace Intense.UI
{
    public static class SpriteAtlasRequestedHandler
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SpriteAtlasManager.atlasRequested -= Ignore;
            SpriteAtlasManager.atlasRequested += Ignore;
        }

        private static void Ignore(string _, Action<SpriteAtlas> __) { }
    }
}