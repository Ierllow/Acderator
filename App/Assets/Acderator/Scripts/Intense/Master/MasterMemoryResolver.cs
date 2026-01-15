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
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(12)
            {
                {typeof(TitleMaster[]), 0 },
                {typeof(SongSelectMaster[]), 1 },
                {typeof(SongMaster[]), 2 },
                {typeof(SongScoreRateMaster[]), 3 },
                {typeof(SongBaseScoreMaster[]), 4 },
                {typeof(SongJudgeZoneMaster[]), 5 },
                {typeof(SongBaseHpMaster[]), 6 },
                {typeof(SongHpRateMaster[]), 7 },
                {typeof(ResultMaster[]), 8 },
                {typeof(SoundSheetNameMaster[]), 9 },
                {typeof(TutorialStepMaster[]), 10 },
                {typeof(TutorialMaster[]), 11 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key)) return null;

            switch (key)
            {
                case 0: return new MessagePack.Formatters.ArrayFormatter<TitleMaster>();
                case 1: return new MessagePack.Formatters.ArrayFormatter<SongSelectMaster>();
                case 2: return new MessagePack.Formatters.ArrayFormatter<SongMaster>();
                case 3: return new MessagePack.Formatters.ArrayFormatter<SongScoreRateMaster>();
                case 4: return new MessagePack.Formatters.ArrayFormatter<SongBaseScoreMaster>();
                case 5: return new MessagePack.Formatters.ArrayFormatter<SongJudgeZoneMaster>();
                case 6: return new MessagePack.Formatters.ArrayFormatter<SongBaseHpMaster>();
                case 7: return new MessagePack.Formatters.ArrayFormatter<SongHpRateMaster>();
                case 8: return new MessagePack.Formatters.ArrayFormatter<ResultMaster>();
                case 9: return new MessagePack.Formatters.ArrayFormatter<SoundSheetNameMaster>();
                case 10: return new MessagePack.Formatters.ArrayFormatter<TutorialStepMaster>();
                case 11: return new MessagePack.Formatters.ArrayFormatter<TutorialMaster>();
                default: return null;
            }
        }
    }
}