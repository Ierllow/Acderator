using Intense.Master;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System;
using Master.Tables;

namespace Master
{
    public class MasterMemoryResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new MasterMemoryResolver();

        MasterMemoryResolver()
        {

        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                var f = MasterMemoryResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class MasterMemoryResolverGetFormatterHelper
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static MasterMemoryResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(13)
            {
                {typeof(VersionMaster[]), 0 },
                {typeof(TitleMaster[]), 1 },
                {typeof(SongSelectMaster[]), 2 },
                {typeof(SongMaster[]), 3 },
                {typeof(SongScoreRateMaster[]), 4 },
                {typeof(SongBaseScoreMaster[]), 5 },
                {typeof(SongJudgeZoneMaster[]), 6 },
                {typeof(SongBaseHpMaster[]), 7 },
                {typeof(SongHpRateMaster[]), 8 },
                {typeof(ResultMaster[]), 9 },
                {typeof(SoundSheetNameMaster[]), 10 },
                {typeof(TutorialStepMaster[]), 11 },
                {typeof(TutorialMaster[]), 12 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key)) return null;

            switch (key)
            {
                case 0: return new MessagePack.Formatters.ArrayFormatter<VersionMaster>();
                case 1: return new MessagePack.Formatters.ArrayFormatter<TitleMaster>();
                case 2: return new MessagePack.Formatters.ArrayFormatter<SongSelectMaster>();
                case 3: return new MessagePack.Formatters.ArrayFormatter<SongMaster>();
                case 4: return new MessagePack.Formatters.ArrayFormatter<SongScoreRateMaster>();
                case 5: return new MessagePack.Formatters.ArrayFormatter<SongBaseScoreMaster>();
                case 6: return new MessagePack.Formatters.ArrayFormatter<SongJudgeZoneMaster>();
                case 7: return new MessagePack.Formatters.ArrayFormatter<SongBaseHpMaster>();
                case 8: return new MessagePack.Formatters.ArrayFormatter<SongHpRateMaster>();
                case 9: return new MessagePack.Formatters.ArrayFormatter<ResultMaster>();
                case 10: return new MessagePack.Formatters.ArrayFormatter<SoundSheetNameMaster>();
                case 11: return new MessagePack.Formatters.ArrayFormatter<TutorialStepMaster>();
                case 12: return new MessagePack.Formatters.ArrayFormatter<TutorialMaster>();
                default: return null;
            }
        }
    }
}