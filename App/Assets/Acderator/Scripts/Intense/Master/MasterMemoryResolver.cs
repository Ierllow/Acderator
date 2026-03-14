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
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(10)
            {
                {typeof(TitleMaster[]), 0 },
                {typeof(SongSelectMaster[]), 1 },
                {typeof(SongMaster[]), 2 },
                {typeof(SongScoreRateMaster[]), 3 },
                {typeof(SongJudgeZoneMaster[]), 4 },
                {typeof(SongHpRateMaster[]), 5 },
                {typeof(ResultMaster[]), 6 },
                {typeof(SoundSheetNameMaster[]), 7 },
                {typeof(TutorialStepMaster[]), 8 },
                {typeof(TutorialMaster[]), 9 },
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
                case 4: return new MessagePack.Formatters.ArrayFormatter<SongJudgeZoneMaster>();
                case 5: return new MessagePack.Formatters.ArrayFormatter<SongHpRateMaster>();
                case 6: return new MessagePack.Formatters.ArrayFormatter<ResultMaster>();
                case 7: return new MessagePack.Formatters.ArrayFormatter<SoundSheetNameMaster>();
                case 8: return new MessagePack.Formatters.ArrayFormatter<TutorialStepMaster>();
                case 9: return new MessagePack.Formatters.ArrayFormatter<TutorialMaster>();
                default: return null;
            }
        }
    }
}