using Cysharp.Threading.Tasks;
using Master;
using Intense.Api;
using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace Intense.Master
{
    internal class MasterDataManager : IInitializable
    {
        [Inject] private readonly IApiSession apiSession;

        public MemoryDatabase MemoryDatabase { get; private set; }

        public void Initialize()
        {
            var resolver = CompositeResolver.Create(MasterMemoryResolver.Instance, GeneratedResolver.Instance, StandardResolver.Instance);
            MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard.WithResolver(resolver);
        }

        public async UniTask LoadMasterAsync(Dictionary<string, object> masterDict)
        {
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
            ValidateMaster(MemoryDatabase);

            await UniTask.CompletedTask;
        }

        private void ValidateMaster(MemoryDatabase database)
        {
            var problems = new List<string>();

            if (!database.ResultMasterTable.All.Any()) problems.Add("result_masters is empty");
            if (!database.SongJudgeZoneMasterTable.All.Any(x => x.Type <= (int)JudgementType.Bad)) problems.Add("judge_zone_masters has no zone for Bad or better");

            foreach (var song in database.SongMasterTable.All)
            {
                if (song.Score <= 0) problems.Add(string.Format("song_masters sid={0} has no score", song.Sid));
                if (song.Hp <= 0) problems.Add(string.Format("song_masters sid={0} has no hp", song.Sid));
            }
            foreach (var judgementType in new[] { JudgementType.Perfect, JudgementType.Great, JudgementType.Good, JudgementType.Bad, JudgementType.Miss })
            {
                if (database.SongHpRateMasterTable.FindByType((int)judgementType) == null) problems.Add(string.Format("hp_rate_masters has no row for {0}", judgementType));
            }

            if (problems.Count > 0) throw new InvalidOperationException("Invalid master data: " + string.Join(", ", problems));
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

        private List<T> ReadMasters<T>(Dictionary<string, object> masterDict, string key, Func<Dictionary<string, object>, T> create) where T : class => masterDict.GetList(key).Select(x => x.TryConvertDictionary(out var dictionary) ? create(dictionary) : default).Where(x => x != null).ToList();

        private Dictionary<string, object> RenameKey(Dictionary<string, object> dictionary, string from, string to)
        {
            if (dictionary.TryGetValue(from, out var value) && !dictionary.ContainsKey(to)) dictionary[to] = value;
            return dictionary;
        }

        private int FirstInt(Dictionary<string, object> masterDict, string listKey, string valueKey) => masterDict.GetList(listKey).Select(x => x.TryConvertDictionary(out var dictionary) && dictionary.TryGetInt(valueKey, out var value) ? value : 0).FirstOrDefault();
    }
}