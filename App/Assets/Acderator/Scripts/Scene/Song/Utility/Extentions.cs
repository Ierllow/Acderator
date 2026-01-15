using Intense.Internal;

namespace Song
{
    internal static class Extensions
    {
        public static SongSceneContext AsSongSceneContext(this SceneContext sceneContext)
        {
            var songSceneContext = sceneContext as SongSceneContext;
            Error.ThrowArgumentNullException(songSceneContext, nameof(songSceneContext));
            return songSceneContext;
        }
    }
}