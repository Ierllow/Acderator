using MasterMemory;
using MessagePack;

namespace Intense.Master
{
    [MessagePackObject(true)]
    public abstract class BaseMaster { }

    [MemoryTable("TitleMaster")]
    public class TitleMaster : BaseMaster
    {
        [PrimaryKey] public int Tid { get; set; }
    }

    [MemoryTable("SongSelectMaster")]
    public class SongSelectMaster : BaseMaster
    {
        [PrimaryKey] public int Group { get; set; }
        public int StartSongTime { get; set; }
        public int SongTime { get; set; }
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
    }

    [MemoryTable("SongScoreRateMaster")]
    public class SongScoreRateMaster : BaseMaster
    {
        [PrimaryKey] public int Type { get; set; }
        public float Rate { get; set; }
    }

    [MemoryTable("SongBaseScoreMaster")]
    public class SongBaseScoreMaster : BaseMaster
    {
        [PrimaryKey] public int Score { get; set; }
    }

    [MemoryTable("SongJudgeZoneMaster")]
    public class SongJudgeZoneMaster : BaseMaster
    {
        [PrimaryKey] public int Type { get; set; }
        public float Zone { get; set; }
    }

    [MemoryTable("SongBaseHpMaster")]
    public class SongBaseHpMaster : BaseMaster
    {
        [PrimaryKey] public int Hp { get; set; }
    }

    [MemoryTable("SongHpRateMaster")]
    public class SongHpRateMaster : BaseMaster
    {
        [PrimaryKey] public int Type { get; set; }
        public int Rate { get; set; }
    }

    [MemoryTable("ResultMaster")]
    public class ResultMaster : BaseMaster
    {
        [PrimaryKey] public int Rid { get; set; }
    }

    [MemoryTable("SoundSheetNameMaster")]
    public class SoundSheetNameMaster : BaseMaster
    {
        [PrimaryKey] public int Category { get; set; }
        [SecondaryKey(0)] public int Id { get; set; }
        public string SheetName { get; set; }
        public string CueName { get; set; }
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
    }
}