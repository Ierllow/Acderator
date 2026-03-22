using MasterMemory;
using MessagePack;
using System;
using System.Collections.Generic;

namespace Intense.Master
{
    [MessagePackObject(true)]
    public abstract class BaseMaster { }

    [MemoryTable("VersionMaster")]
    public class VersionMaster : BaseMaster
    {
        [PrimaryKey] public int Version { get; set; }

        public static VersionMaster From(Dictionary<string, object> masterDict) => new()
        {
            Version = masterDict.TryGetValue("version", out var version) ? int.Parse(version.ToString()) : 0,
        };
    }

    [MemoryTable("TitleMaster")]
    public class TitleMaster : BaseMaster
    {
        [PrimaryKey] public int Tid { get; set; }

        public static TitleMaster From(Dictionary<string, object> masterDict) => new()
        {
            Tid = masterDict.TryGetValue("tid", out var tid) ? int.Parse(tid.ToString()) : 0,
        };
    }

    [MemoryTable("SongSelectMaster")]
    public class SongSelectMaster : BaseMaster
    {
        [PrimaryKey] public int Group { get; set; }
        public int StartSongTime { get; set; }
        public int SongTime { get; set; }

        public static SongSelectMaster From(Dictionary<string, object> masterDict) => new SongSelectMaster
        {
            Group = masterDict.TryGetValue("group", out var group) ? int.Parse(group.ToString()) : 0,
            StartSongTime = masterDict.TryGetValue("start_song_time", out var startSongTime) ? int.Parse(startSongTime.ToString()) : 0,
            SongTime = masterDict.TryGetValue("song_time", out var songTime) ? int.Parse(songTime.ToString()) : 0,
        };
    }

    [MemoryTable("SongMaster")]
    public class SongMaster : BaseMaster
    {
        [PrimaryKey] public int Sid { get; set; }
        [SecondaryKey(0)] public int Group { get; set; }
        public int Difficulty { get; set; }
        public string Name { get; set; }
        public string Composer { get; set; }
        public float Start_offset { get; set; }
        public int Bg { get; set; }
        public int Score { get; set; }
        public int Hp { get; set; }

        public static SongMaster From(Dictionary<string, object> masterDict) => new()
        {
            Sid = masterDict.TryGetValue("sid", out var sid) ? int.Parse(sid.ToString()) : 0,
            Group = masterDict.TryGetValue("group", out var group) ? int.Parse(group.ToString()) : 0,
            Difficulty = masterDict.TryGetValue("difficulty", out var difficulty) ? int.Parse(difficulty.ToString()) : 0,
            Name = masterDict.TryGetValue("name", out var name) ? name.ToString() : string.Empty,
            Composer = masterDict.TryGetValue("composer", out var composer) ? composer.ToString() : string.Empty,
            Start_offset = masterDict.TryGetValue("start_offset", out var startOffset) ? float.Parse(startOffset.ToString()) : 0f,
            Bg = masterDict.TryGetValue("bg", out var bg) ? int.Parse(bg.ToString()) : 0,
            Score = masterDict.TryGetValue("score", out var score) ? int.Parse(score.ToString()) : 0,
            Hp = masterDict.TryGetValue("hp", out var hp) ? int.Parse(hp.ToString()) : 0,
        };
    }

    [MemoryTable("SongScoreRateMaster")]
    public class SongScoreRateMaster : BaseMaster
    {
        [PrimaryKey] public int Type { get; set; }
        public float Rate { get; set; }

        public static SongScoreRateMaster From(Dictionary<string, object> masterDict) => new()
        {
            Type = masterDict.TryGetValue("type", out var type) ? int.Parse(type.ToString()) : 0,
            Rate = masterDict.TryGetValue("rate", out var rate) ? float.Parse(rate.ToString()) : 0f,
        };
    }

    [Obsolete("Will be delete in the future.")]
    [MemoryTable("SongBaseScoreMaster")]
    public class SongBaseScoreMaster
    {
        [PrimaryKey] public int Score { get; set; }
    }

    [MemoryTable("SongJudgeZoneMaster")]
    public class SongJudgeZoneMaster : BaseMaster
    {
        [PrimaryKey] public int Type { get; set; }
        public float Zone { get; set; }

        public static SongJudgeZoneMaster From(Dictionary<string, object> masterDict) => new()
        {
            Type = masterDict.TryGetValue("type", out var type) ? int.Parse(type.ToString()) : 0,
            Zone = masterDict.TryGetValue("zone", out var zone) ? float.Parse(zone.ToString()) : 0f,
        };
    }

    [Obsolete("Will be delete in the future.")]
    [MemoryTable("SongBaseHpMaster")]
    public class SongBaseHpMaster
    {
        [PrimaryKey] public int Hp { get; set; }
    }

    [MemoryTable("SongHpRateMaster")]
    public class SongHpRateMaster : BaseMaster
    {
        [PrimaryKey] public int Type { get; set; }
        public int Rate { get; set; }

        public static SongHpRateMaster From(Dictionary<string, object> masterDict) => new()
        {
            Type = masterDict.TryGetValue("type", out var type) ? int.Parse(type.ToString()) : 0,
            Rate = masterDict.TryGetValue("rate", out var rate) ? int.Parse(rate.ToString()) : 0,
        };
    }

    [MemoryTable("ResultMaster")]
    public class ResultMaster : BaseMaster
    {
        [PrimaryKey] public int Rid { get; set; }

        public static ResultMaster From(Dictionary<string, object> masterDict) => new()
        {
            Rid = masterDict.TryGetValue("rid", out var rid) ? int.Parse(rid.ToString()) : 0,
        };
    }

    [MemoryTable("SoundSheetNameMaster")]
    public class SoundSheetNameMaster : BaseMaster
    {
        [PrimaryKey] public int Category { get; set; }
        [SecondaryKey(0)] public int Id { get; set; }
        public string SheetName { get; set; }
        public string CueName { get; set; }

        public static SoundSheetNameMaster From(Dictionary<string, object> masterDict) => new()
        {
            Category = masterDict.TryGetValue("category", out var category) ? int.Parse(category.ToString()) : 0,
            Id = masterDict.TryGetValue("id", out var id) ? int.Parse(id.ToString()) : 0,
            SheetName = masterDict.TryGetValue("sheet_name", out var sheetName) ? sheetName.ToString() : string.Empty,
            CueName = masterDict.TryGetValue("cue_name", out var cueName) ? cueName.ToString() : string.Empty,
        };
    }

    [MemoryTable("TutorialStepMaster")]
    public class TutorialStepMaster : BaseMaster
    {
        [PrimaryKey] public int Tid { get; set; }
        public int StepOrder { get; set; }
        public string Description { get; set; }
        public float TriggerTime { get; set; }
        public float HintPositionX { get; set; }
        public float HintPositionY { get; set; }
        public bool IsSkippable { get; set; }

        public static TutorialStepMaster From(Dictionary<string, object> masterDict) => new()
        {
            Tid = masterDict.TryGetValue("tid", out var tid) ? int.Parse(tid.ToString()) : 0,
            StepOrder = masterDict.TryGetValue("step_order", out var stepOrder) ? int.Parse(stepOrder.ToString()) : 0,
            Description = masterDict.TryGetValue("description", out var description) ? description.ToString() : string.Empty,
            TriggerTime = masterDict.TryGetValue("trigger_time", out var triggerTime) ? float.Parse(triggerTime.ToString()) : 0f,
            HintPositionX = masterDict.TryGetValue("hint_position_x", out var hintPositionX) ? float.Parse(hintPositionX.ToString()) : 0f,
            HintPositionY = masterDict.TryGetValue("hint_position_y", out var hintPositionY) ? float.Parse(hintPositionY.ToString()) : 0f,
            IsSkippable = masterDict.TryGetValue("is_skippable", out var isSkippable) && bool.Parse(isSkippable.ToString()),
        };
    }

    [MemoryTable("TutorialMaster")]
    public class TutorialMaster : BaseMaster
    {
        [PrimaryKey] public int Tid { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Sid { get; set; }
        public int Order { get; set; }
        public bool IsRequired { get; set; }
        public string VoiceCue { get; set; }
        public string HintText { get; set; }
        public int Type { get; set; }

        public static TutorialMaster From(Dictionary<string, object> masterDict) => new()
        {
            Tid = masterDict.TryGetValue("tid", out var tid) ? int.Parse(tid.ToString()) : 0,
            Title = masterDict.TryGetValue("title", out var title) ? title.ToString() : string.Empty,
            Description = masterDict.TryGetValue("description", out var description) ? description.ToString() : string.Empty,
            Sid = masterDict.TryGetValue("sid", out var sid) ? int.Parse(sid.ToString()) : 0,
            Order = masterDict.TryGetValue("order", out var order) ? int.Parse(order.ToString()) : 0,
            IsRequired = masterDict.TryGetValue("is_required", out var isRequired) && bool.Parse(isRequired.ToString()),
            VoiceCue = masterDict.TryGetValue("voice_cue", out var voiceCue) ? voiceCue.ToString() : string.Empty,
            HintText = masterDict.TryGetValue("hint_text", out var hintText) ? hintText.ToString() : string.Empty,
            Type = masterDict.TryGetValue("type", out var type) ? int.Parse(type.ToString()) : 0,
        };
    }
}