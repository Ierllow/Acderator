using Intense.Master;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System;
using Master.Tables;

namespace Master
{
   public sealed class MemoryDatabase : MemoryDatabaseBase
   {
        public VersionMasterTable VersionMasterTable { get; private set; }
        public TitleMasterTable TitleMasterTable { get; private set; }
        public SongSelectMasterTable SongSelectMasterTable { get; private set; }
        public SongMasterTable SongMasterTable { get; private set; }
        public SongScoreRateMasterTable SongScoreRateMasterTable { get; private set; }
        public SongJudgeZoneMasterTable SongJudgeZoneMasterTable { get; private set; }
        public SongHpRateMasterTable SongHpRateMasterTable { get; private set; }
        public ResultMasterTable ResultMasterTable { get; private set; }
        public SoundSheetNameMasterTable SoundSheetNameMasterTable { get; private set; }
        public TutorialStepMasterTable TutorialStepMasterTable { get; private set; }
        public TutorialMasterTable TutorialMasterTable { get; private set; }

        public MemoryDatabase(
            VersionMasterTable VersionMasterTable,
            TitleMasterTable TitleMasterTable,
            SongSelectMasterTable SongSelectMasterTable,
            SongMasterTable SongMasterTable,
            SongScoreRateMasterTable SongScoreRateMasterTable,
            SongJudgeZoneMasterTable SongJudgeZoneMasterTable,
            SongHpRateMasterTable SongHpRateMasterTable,
            ResultMasterTable ResultMasterTable,
            SoundSheetNameMasterTable SoundSheetNameMasterTable,
            TutorialStepMasterTable TutorialStepMasterTable,
            TutorialMasterTable TutorialMasterTable
        )
        {
            this.VersionMasterTable = VersionMasterTable;
            this.TitleMasterTable = TitleMasterTable;
            this.SongSelectMasterTable = SongSelectMasterTable;
            this.SongMasterTable = SongMasterTable;
            this.SongScoreRateMasterTable = SongScoreRateMasterTable;
            this.SongJudgeZoneMasterTable = SongJudgeZoneMasterTable;
            this.SongHpRateMasterTable = SongHpRateMasterTable;
            this.ResultMasterTable = ResultMasterTable;
            this.SoundSheetNameMasterTable = SoundSheetNameMasterTable;
            this.TutorialStepMasterTable = TutorialStepMasterTable;
            this.TutorialMasterTable = TutorialMasterTable;
        }

        public MemoryDatabase(byte[] databaseBinary, bool internString = true, MessagePack.IFormatterResolver formatterResolver = null)
            : base(databaseBinary, internString, formatterResolver)
        {
        }

        protected override void Init(Dictionary<string, (int offset, int count)> header, int headerOffset, byte[] databaseBinary, MessagePack.IFormatterResolver resolver)
        {
            this.VersionMasterTable = ExtractTableData<VersionMaster, VersionMasterTable>(header, headerOffset, databaseBinary, resolver, xs => new VersionMasterTable(xs));
            this.TitleMasterTable = ExtractTableData<TitleMaster, TitleMasterTable>(header, headerOffset, databaseBinary, resolver, xs => new TitleMasterTable(xs));
            this.SongSelectMasterTable = ExtractTableData<SongSelectMaster, SongSelectMasterTable>(header, headerOffset, databaseBinary, resolver, xs => new SongSelectMasterTable(xs));
            this.SongMasterTable = ExtractTableData<SongMaster, SongMasterTable>(header, headerOffset, databaseBinary, resolver, xs => new SongMasterTable(xs));
            this.SongScoreRateMasterTable = ExtractTableData<SongScoreRateMaster, SongScoreRateMasterTable>(header, headerOffset, databaseBinary, resolver, xs => new SongScoreRateMasterTable(xs));
            this.SongJudgeZoneMasterTable = ExtractTableData<SongJudgeZoneMaster, SongJudgeZoneMasterTable>(header, headerOffset, databaseBinary, resolver, xs => new SongJudgeZoneMasterTable(xs));
            this.SongHpRateMasterTable = ExtractTableData<SongHpRateMaster, SongHpRateMasterTable>(header, headerOffset, databaseBinary, resolver, xs => new SongHpRateMasterTable(xs));
            this.ResultMasterTable = ExtractTableData<ResultMaster, ResultMasterTable>(header, headerOffset, databaseBinary, resolver, xs => new ResultMasterTable(xs));
            this.SoundSheetNameMasterTable = ExtractTableData<SoundSheetNameMaster, SoundSheetNameMasterTable>(header, headerOffset, databaseBinary, resolver, xs => new SoundSheetNameMasterTable(xs));
            this.TutorialStepMasterTable = ExtractTableData<TutorialStepMaster, TutorialStepMasterTable>(header, headerOffset, databaseBinary, resolver, xs => new TutorialStepMasterTable(xs));
            this.TutorialMasterTable = ExtractTableData<TutorialMaster, TutorialMasterTable>(header, headerOffset, databaseBinary, resolver, xs => new TutorialMasterTable(xs));
        }

        public ImmutableBuilder ToImmutableBuilder()
        {
            return new ImmutableBuilder(this);
        }

        public DatabaseBuilder ToDatabaseBuilder()
        {
            var builder = new DatabaseBuilder();
            builder.Append(this.VersionMasterTable.GetRawDataUnsafe());
            builder.Append(this.TitleMasterTable.GetRawDataUnsafe());
            builder.Append(this.SongSelectMasterTable.GetRawDataUnsafe());
            builder.Append(this.SongMasterTable.GetRawDataUnsafe());
            builder.Append(this.SongScoreRateMasterTable.GetRawDataUnsafe());
            builder.Append(this.SongJudgeZoneMasterTable.GetRawDataUnsafe());
            builder.Append(this.SongHpRateMasterTable.GetRawDataUnsafe());
            builder.Append(this.ResultMasterTable.GetRawDataUnsafe());
            builder.Append(this.SoundSheetNameMasterTable.GetRawDataUnsafe());
            builder.Append(this.TutorialStepMasterTable.GetRawDataUnsafe());
            builder.Append(this.TutorialMasterTable.GetRawDataUnsafe());
            return builder;
        }
    }
}