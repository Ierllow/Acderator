using Cysharp.Threading.Tasks;
using Master;
using Intense.Api;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace Intense.Master
{
    internal class MasterDataManager : IInitializable
    {
        [Inject] private IApiSession apiSession;

        public MemoryDatabase MemoryDatabase { get; private set; }

        public void Initialize() => CompositeResolver.RegisterAndSetAsDefault(new[] { MasterMemoryResolver.Instance, GeneratedResolver.Instance, StandardResolver.Instance });

        public async UniTask LoadMasterAsync(Dictionary<string, object> masterDict)
        {
            var completionSource = AutoResetUniTaskCompletionSource.Create();
            var builder = new DatabaseBuilder();
            var baseScore = FirstInt(masterDict, "base_score_masters", "score");
            var baseHp = FirstInt(masterDict, "base_hp_masters", "hp");

            builder.Append(SetVersion(masterDict));
            builder.Append(ReadMasters(masterDict, "title_masters", TitleMaster.From));
            builder.Append(ReadMasters(masterDict, "song_masters", x =>
            {
                if (!x.ContainsKey("score")) x["score"] = baseScore;
                if (!x.ContainsKey("hp")) x["hp"] = baseHp;
                return SongMaster.From(x);
            }));
            builder.Append(ReadMasters(masterDict, "song_select_masters", SongSelectMaster.From));
            builder.Append(ReadMasters(masterDict, "score_rate_masters", x => SongScoreRateMaster.From(RenameKey(x, "r_type", "type"))));
            builder.Append(ReadMasters(masterDict, "judge_zone_masters", x => SongJudgeZoneMaster.From(RenameKey(x, "j_type", "type"))));
            builder.Append(ReadMasters(masterDict, "hp_rate_masters", x => SongHpRateMaster.From(RenameKey(x, "j_type", "type"))));
            builder.Append(ReadMasters(masterDict, "result_masters", ResultMaster.From));
            builder.Append(ReadMasters(masterDict, "sound_sheet_masters", SoundSheetNameMaster.From));
            builder.Append(ReadMasters(masterDict, "tutorial_step_masters", TutorialStepMaster.From));
            builder.Append(ReadMasters(masterDict, "tutorial_masters", TutorialMaster.From));
            MemoryDatabase = new(builder.Build());
            completionSource.TrySetResult();

            await completionSource.Task;
        }

        private List<VersionMaster> SetVersion(Dictionary<string, object> masterDict)
        {
            if (!masterDict.TryGetString("version_master", out var version)) return new();

            apiSession.MasterVersion = version;
            return new()
            {
                new()
                {
                    Version = int.TryParse(version, out var versionNumber) ? versionNumber : 0,
                },
            };
        }

        private static List<T> ReadMasters<T>(Dictionary<string, object> masterDict, string key, Func<Dictionary<string, object>, T> create) where T : class => masterDict.GetList(key).Select(x => x.TryConvertDictionary(out var dictionary) ? create(dictionary) : default).Where(x => x != null).ToList();

        private static Dictionary<string, object> RenameKey(Dictionary<string, object> dictionary, string from, string to)
        {
            if (dictionary.TryGetValue(from, out var value) && !dictionary.ContainsKey(to)) dictionary[to] = value;
            return dictionary;
        }

        private static int FirstInt(Dictionary<string, object> masterDict, string listKey, string valueKey) => masterDict.GetList(listKey).Select(x => x.TryConvertDictionary(out var dictionary) && dictionary.TryGetInt(valueKey, out var value) ? value : 0).FirstOrDefault();
    }
}