using System.Collections.Generic;

namespace Result
{
    public sealed class ResultSceneContext : SceneContext
    {
        public ResultInfo ResultInfo { get; init; }

        public List<string> ResultSceneBundlePathList => new()
        {
            "song/jacket",
            "song/rank",
            "result/bg",
            "sounds/bgm/bgm",
        };
    }
}