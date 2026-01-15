using Intense.Master;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System;
using Master.Tables;

namespace Master
{
   public sealed class DatabaseBuilder : DatabaseBuilderBase
   {
        public DatabaseBuilder() : this(null) { }
        public DatabaseBuilder(MessagePack.IFormatterResolver resolver) : base(resolver) { }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<TitleMaster> dataSource)
        {
            AppendCore(dataSource, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            return this;
        }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<SongSelectMaster> dataSource)
        {
            AppendCore(dataSource, x => x.Group, System.Collections.Generic.Comparer<int>.Default);
            return this;
        }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<SongMaster> dataSource)
        {
            AppendCore(dataSource, x => x.Sid, System.Collections.Generic.Comparer<int>.Default);
            return this;
        }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<SongScoreRateMaster> dataSource)
        {
            AppendCore(dataSource, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            return this;
        }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<SongBaseScoreMaster> dataSource)
        {
            AppendCore(dataSource, x => x.Score, System.Collections.Generic.Comparer<int>.Default);
            return this;
        }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<SongJudgeZoneMaster> dataSource)
        {
            AppendCore(dataSource, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            return this;
        }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<SongBaseHpMaster> dataSource)
        {
            AppendCore(dataSource, x => x.Hp, System.Collections.Generic.Comparer<int>.Default);
            return this;
        }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<SongHpRateMaster> dataSource)
        {
            AppendCore(dataSource, x => x.Type, System.Collections.Generic.Comparer<int>.Default);
            return this;
        }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<ResultMaster> dataSource)
        {
            AppendCore(dataSource, x => x.Rid, System.Collections.Generic.Comparer<int>.Default);
            return this;
        }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<SoundSheetNameMaster> dataSource)
        {
            AppendCore(dataSource, x => x.Category, System.Collections.Generic.Comparer<int>.Default);
            return this;
        }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<TutorialStepMaster> dataSource)
        {
            AppendCore(dataSource, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            return this;
        }

        public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<TutorialMaster> dataSource)
        {
            AppendCore(dataSource, x => x.Tid, System.Collections.Generic.Comparer<int>.Default);
            return this;
        }

    }
}