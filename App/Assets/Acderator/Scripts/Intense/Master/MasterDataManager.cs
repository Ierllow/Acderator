using Cysharp.Threading.Tasks;
using Master;
using MessagePack.Resolvers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Intense.Master
{
    internal class MasterDataManager : IInitializable
    {
        public MemoryDatabase MemoryDatabase { get; private set; }

        public void Initialize() => CompositeResolver.RegisterAndSetAsDefault(new[] { MasterMemoryResolver.Instance, GeneratedResolver.Instance, StandardResolver.Instance });

        public async UniTask LoadMasterAsync(Dictionary<string, object> masterDict)
        {
            var completionSource = AutoResetUniTaskCompletionSource.Create();
            var builder = new DatabaseBuilder();
            builder.Append(masterDict.TryGetValue("version", out var version) ? (version as Dictionary<string, object>).Select(x => VersionMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            builder.Append(masterDict.TryGetValue("title_masters", out var titleMasters) ? (titleMasters as Dictionary<string, object>).Select(x => TitleMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            builder.Append(masterDict.TryGetValue("song_masters", out var songMasters) ? (songMasters as Dictionary<string, object>).Select(x => SongMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            builder.Append(masterDict.TryGetValue("song_select_masters", out var songSelectMasters) ? (songSelectMasters as Dictionary<string, object>).Select(x => SongSelectMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            builder.Append(masterDict.TryGetValue("song_score_rate_masters", out var songScoreRateMasters) ? (songScoreRateMasters as Dictionary<string, object>).Select(x => SongScoreRateMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            builder.Append(masterDict.TryGetValue("song_judge_zone_masters", out var songJudgeZoneMasters) ? (songJudgeZoneMasters as Dictionary<string, object>).Select(x => SongJudgeZoneMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            builder.Append(masterDict.TryGetValue("song_hp_rate_masters", out var songHpRateMasters) ? (songHpRateMasters as Dictionary<string, object>).Select(x => SongHpRateMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            builder.Append(masterDict.TryGetValue("result_masters", out var resultMasters) ? (resultMasters as Dictionary<string, object>).Select(x => ResultMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            builder.Append(masterDict.TryGetValue("sound_sheet_name_masters", out var soundSheetNameMasters) ? (soundSheetNameMasters as Dictionary<string, object>).Select(x => SoundSheetNameMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            MemoryDatabase = new(builder.Build());
            completionSource.TrySetResult();

            await completionSource.Task;
        }
    }
}