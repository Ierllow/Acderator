using System;
using System.Collections.Generic;
using System.Linq;
using Master;

namespace Intense.Master
{
    internal static class MasterDataValidator
    {
        public static void Validate(MemoryDatabase database)
        {
            var problemList = new List<string>();

            if (!database.ResultMasterTable.All.Any()) problemList.Add("result_masters is empty");
            if (!database.SongJudgeZoneMasterTable.All.Any(x => x.Type <= (int)JudgementType.Bad)) problemList.Add("judge_zone_masters has no zone for Bad or better");

            foreach (var song in database.SongMasterTable.All)
            {
                if (song.Score <= 0) problemList.Add(string.Format("song_masters sid={0} has no score", song.Sid));
                if (song.Hp <= 0) problemList.Add(string.Format("song_masters sid={0} has no hp", song.Sid));
            }
            foreach (var judgementType in new[] { JudgementType.Perfect, JudgementType.Great, JudgementType.Good, JudgementType.Bad, JudgementType.Miss })
            {
                if (database.SongHpRateMasterTable.FindByType((int)judgementType) == null) problemList.Add(string.Format("hp_rate_masters has no row for {0}", judgementType));
            }

            if (problemList.Count > 0) throw new InvalidOperationException("Invalid master data: " + string.Join(", ", problemList));
        }
    }
}