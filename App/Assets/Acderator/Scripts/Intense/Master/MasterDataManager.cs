using Cysharp.Threading.Tasks;
using Master;
using MessagePack.Resolvers;
using System.Collections.Generic;
using ZLinq;

namespace Intense.Master
{
    internal class MasterDataManager : SingletonMonoBehaviour<MasterDataManager>
    {
        public bool IsInit { get; private set; } = false;

        internal MemoryDatabase MemoryDatabase { get; private set; }

        protected override void Awake()
        {
            if (IsInit) return;
            CompositeResolver.RegisterAndSetAsDefault(new[] { MasterMemoryResolver.Instance, GeneratedResolver.Instance, StandardResolver.Instance });
            base.Awake();
        }

        public async UniTask LoadMasterAsync(Dictionary<string, object> masterDict)
        {
            var builder = new DatabaseBuilder();
            builder.Append(masterDict.TryGetValue("version", out var version) ? (version as Dictionary<string, object>).AsValueEnumerable().Select(x => VersionMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            builder.Append(masterDict.TryGetValue("title_masters", out var titleMasters) ? (titleMasters as Dictionary<string, object>).AsValueEnumerable().Select(x => TitleMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            builder.Append(masterDict.TryGetValue("song_masters", out var songMasters) ? (songMasters as Dictionary<string, object>).AsValueEnumerable().Select(x => SongMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            builder.Append(masterDict.TryGetValue("song_select_masters", out var songSelectMasters) ? (songSelectMasters as Dictionary<string, object>).AsValueEnumerable().Select(x => SongSelectMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            builder.Append(masterDict.TryGetValue("song_score_rate_masters", out var songScoreRateMasters) ? (songScoreRateMasters as Dictionary<string, object>).AsValueEnumerable().Select(x => SongScoreRateMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            builder.Append(masterDict.TryGetValue("song_judge_zone_masters", out var songJudgeZoneMasters) ? (songJudgeZoneMasters as Dictionary<string, object>).AsValueEnumerable().Select(x => SongJudgeZoneMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            builder.Append(masterDict.TryGetValue("song_hp_rate_masters", out var songHpRateMasters) ? (songHpRateMasters as Dictionary<string, object>).AsValueEnumerable().Select(x => SongHpRateMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            builder.Append(masterDict.TryGetValue("result_masters", out var resultMasters) ? (resultMasters as Dictionary<string, object>).AsValueEnumerable().Select(x => ResultMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            builder.Append(masterDict.TryGetValue("sound_sheet_name_masters", out var soundSheetNameMasters) ? (soundSheetNameMasters as Dictionary<string, object>).AsValueEnumerable().Select(x => SoundSheetNameMaster.From((Dictionary<string, object>)x.Value)).ToList() : default);
            MemoryDatabase = new(builder.Build());

            IsInit = true;
            await UniTask.WaitUntil(() => IsInit);
        }
    }
}