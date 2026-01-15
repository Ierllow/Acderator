using Intense.Master;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System;
using Master.Tables;

namespace Master
{
   public sealed class ImmutableBuilder : ImmutableBuilderBase
   {
        MemoryDatabase memory;

        public ImmutableBuilder(MemoryDatabase memory)
        {
            this.memory = memory;
        }

        public MemoryDatabase Build()
        {
            return memory;
        }

        public void ReplaceAll(System.Collections.Generic.IList<TitleMaster> data)
        {
            var newData = CloneAndSortBy(data, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            var table = new TitleMasterTable(newData);
            memory = new MemoryDatabase(
                table,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void RemoveTitleMaster(int[] keys)
        {
            var data = RemoveCore(memory.TitleMasterTable.GetRawDataUnsafe(), keys, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            var table = new TitleMasterTable(newData);
            memory = new MemoryDatabase(
                table,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void Diff(TitleMaster[] addOrReplaceData)
        {
            var data = DiffCore(memory.TitleMasterTable.GetRawDataUnsafe(), addOrReplaceData, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            var table = new TitleMasterTable(newData);
            memory = new MemoryDatabase(
                table,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<SongSelectMaster> data)
        {
            var newData = CloneAndSortBy(data, x => x.Group, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongSelectMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                table,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void RemoveSongSelectMaster(int[] keys)
        {
            var data = RemoveCore(memory.SongSelectMasterTable.GetRawDataUnsafe(), keys, x => x.Group, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Group, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongSelectMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                table,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void Diff(SongSelectMaster[] addOrReplaceData)
        {
            var data = DiffCore(memory.SongSelectMasterTable.GetRawDataUnsafe(), addOrReplaceData, x => x.Group, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Group, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongSelectMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                table,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<SongMaster> data)
        {
            var newData = CloneAndSortBy(data, x => x.Sid, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                table,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void RemoveSongMaster(int[] keys)
        {
            var data = RemoveCore(memory.SongMasterTable.GetRawDataUnsafe(), keys, x => x.Sid, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Sid, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                table,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void Diff(SongMaster[] addOrReplaceData)
        {
            var data = DiffCore(memory.SongMasterTable.GetRawDataUnsafe(), addOrReplaceData, x => x.Sid, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Sid, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                table,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<SongScoreRateMaster> data)
        {
            var newData = CloneAndSortBy(data, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongScoreRateMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                table,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void RemoveSongScoreRateMaster(int[] keys)
        {
            var data = RemoveCore(memory.SongScoreRateMasterTable.GetRawDataUnsafe(), keys, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongScoreRateMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                table,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void Diff(SongScoreRateMaster[] addOrReplaceData)
        {
            var data = DiffCore(memory.SongScoreRateMasterTable.GetRawDataUnsafe(), addOrReplaceData, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongScoreRateMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                table,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<SongBaseScoreMaster> data)
        {
            var newData = CloneAndSortBy(data, x => x.Score, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongBaseScoreMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                table,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void RemoveSongBaseScoreMaster(int[] keys)
        {
            var data = RemoveCore(memory.SongBaseScoreMasterTable.GetRawDataUnsafe(), keys, x => x.Score, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Score, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongBaseScoreMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                table,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void Diff(SongBaseScoreMaster[] addOrReplaceData)
        {
            var data = DiffCore(memory.SongBaseScoreMasterTable.GetRawDataUnsafe(), addOrReplaceData, x => x.Score, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Score, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongBaseScoreMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                table,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<SongJudgeZoneMaster> data)
        {
            var newData = CloneAndSortBy(data, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongJudgeZoneMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                table,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void RemoveSongJudgeZoneMaster(int[] keys)
        {
            var data = RemoveCore(memory.SongJudgeZoneMasterTable.GetRawDataUnsafe(), keys, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongJudgeZoneMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                table,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void Diff(SongJudgeZoneMaster[] addOrReplaceData)
        {
            var data = DiffCore(memory.SongJudgeZoneMasterTable.GetRawDataUnsafe(), addOrReplaceData, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongJudgeZoneMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                table,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<SongBaseHpMaster> data)
        {
            var newData = CloneAndSortBy(data, x => x.Hp, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongBaseHpMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                table,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void RemoveSongBaseHpMaster(int[] keys)
        {
            var data = RemoveCore(memory.SongBaseHpMasterTable.GetRawDataUnsafe(), keys, x => x.Hp, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Hp, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongBaseHpMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                table,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void Diff(SongBaseHpMaster[] addOrReplaceData)
        {
            var data = DiffCore(memory.SongBaseHpMasterTable.GetRawDataUnsafe(), addOrReplaceData, x => x.Hp, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Hp, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongBaseHpMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                table,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<SongHpRateMaster> data)
        {
            var newData = CloneAndSortBy(data, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongHpRateMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                table,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void RemoveSongHpRateMaster(int[] keys)
        {
            var data = RemoveCore(memory.SongHpRateMasterTable.GetRawDataUnsafe(), keys, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongHpRateMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                table,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void Diff(SongHpRateMaster[] addOrReplaceData)
        {
            var data = DiffCore(memory.SongHpRateMasterTable.GetRawDataUnsafe(), addOrReplaceData, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            var table = new SongHpRateMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                table,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<ResultMaster> data)
        {
            var newData = CloneAndSortBy(data, x => x.Rid, System.Collections.Generic.Comparer<int>.Default);
            var table = new ResultMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                table,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void RemoveResultMaster(int[] keys)
        {
            var data = RemoveCore(memory.ResultMasterTable.GetRawDataUnsafe(), keys, x => x.Rid, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Rid, System.Collections.Generic.Comparer<int>.Default);
            var table = new ResultMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                table,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void Diff(ResultMaster[] addOrReplaceData)
        {
            var data = DiffCore(memory.ResultMasterTable.GetRawDataUnsafe(), addOrReplaceData, x => x.Rid, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Rid, System.Collections.Generic.Comparer<int>.Default);
            var table = new ResultMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                table,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<SoundSheetNameMaster> data)
        {
            var newData = CloneAndSortBy(data, x => x.Category, System.Collections.Generic.Comparer<int>.Default);
            var table = new SoundSheetNameMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                table,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void RemoveSoundSheetNameMaster(int[] keys)
        {
            var data = RemoveCore(memory.SoundSheetNameMasterTable.GetRawDataUnsafe(), keys, x => x.Category, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Category, System.Collections.Generic.Comparer<int>.Default);
            var table = new SoundSheetNameMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                table,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void Diff(SoundSheetNameMaster[] addOrReplaceData)
        {
            var data = DiffCore(memory.SoundSheetNameMasterTable.GetRawDataUnsafe(), addOrReplaceData, x => x.Category, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Category, System.Collections.Generic.Comparer<int>.Default);
            var table = new SoundSheetNameMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                table,
                memory.TutorialStepMasterTable,
                memory.TutorialMasterTable
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<TutorialStepMaster> data)
        {
            var newData = CloneAndSortBy(data, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            var table = new TutorialStepMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                table,
                memory.TutorialMasterTable
            
            );
        }

        public void RemoveTutorialStepMaster(int[] keys)
        {
            var data = RemoveCore(memory.TutorialStepMasterTable.GetRawDataUnsafe(), keys, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            var table = new TutorialStepMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                table,
                memory.TutorialMasterTable
            
            );
        }

        public void Diff(TutorialStepMaster[] addOrReplaceData)
        {
            var data = DiffCore(memory.TutorialStepMasterTable.GetRawDataUnsafe(), addOrReplaceData, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            var table = new TutorialStepMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                table,
                memory.TutorialMasterTable
            
            );
        }

        public void ReplaceAll(System.Collections.Generic.IList<TutorialMaster> data)
        {
            var newData = CloneAndSortBy(data, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            var table = new TutorialMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                table
            
            );
        }

        public void RemoveTutorialMaster(int[] keys)
        {
            var data = RemoveCore(memory.TutorialMasterTable.GetRawDataUnsafe(), keys, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            var table = new TutorialMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                table
            
            );
        }

        public void Diff(TutorialMaster[] addOrReplaceData)
        {
            var data = DiffCore(memory.TutorialMasterTable.GetRawDataUnsafe(), addOrReplaceData, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            var table = new TutorialMasterTable(newData);
            memory = new MemoryDatabase(
                memory.TitleMasterTable,
                memory.SongSelectMasterTable,
                memory.SongMasterTable,
                memory.SongScoreRateMasterTable,
                memory.SongBaseScoreMasterTable,
                memory.SongJudgeZoneMasterTable,
                memory.SongBaseHpMasterTable,
                memory.SongHpRateMasterTable,
                memory.ResultMasterTable,
                memory.SoundSheetNameMasterTable,
                memory.TutorialStepMasterTable,
                table
            
            );
        }

    }
}