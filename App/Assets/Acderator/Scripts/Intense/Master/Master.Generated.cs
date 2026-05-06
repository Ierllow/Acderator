#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Resolvers
{
    using System;
    using MessagePack;

    public class GeneratedResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new GeneratedResolver();

        GeneratedResolver()
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
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(12)
            {
                {typeof(global::Intense.Master.BaseMaster), 0 },
                {typeof(global::Intense.Master.VersionMaster), 1 },
                {typeof(global::Intense.Master.TitleMaster), 2 },
                {typeof(global::Intense.Master.SongSelectMaster), 3 },
                {typeof(global::Intense.Master.SongMaster), 4 },
                {typeof(global::Intense.Master.SongScoreRateMaster), 5 },
                {typeof(global::Intense.Master.SongJudgeZoneMaster), 6 },
                {typeof(global::Intense.Master.SongHpRateMaster), 7 },
                {typeof(global::Intense.Master.ResultMaster), 8 },
                {typeof(global::Intense.Master.SoundSheetNameMaster), 9 },
                {typeof(global::Intense.Master.TutorialStepMaster), 10 },
                {typeof(global::Intense.Master.TutorialMaster), 11 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key)) return null;

            switch (key)
            {
                case 0: return new MessagePack.Formatters.Intense.Master.BaseMasterFormatter();
                case 1: return new MessagePack.Formatters.Intense.Master.VersionMasterFormatter();
                case 2: return new MessagePack.Formatters.Intense.Master.TitleMasterFormatter();
                case 3: return new MessagePack.Formatters.Intense.Master.SongSelectMasterFormatter();
                case 4: return new MessagePack.Formatters.Intense.Master.SongMasterFormatter();
                case 5: return new MessagePack.Formatters.Intense.Master.SongScoreRateMasterFormatter();
                case 6: return new MessagePack.Formatters.Intense.Master.SongJudgeZoneMasterFormatter();
                case 7: return new MessagePack.Formatters.Intense.Master.SongHpRateMasterFormatter();
                case 8: return new MessagePack.Formatters.Intense.Master.ResultMasterFormatter();
                case 9: return new MessagePack.Formatters.Intense.Master.SoundSheetNameMasterFormatter();
                case 10: return new MessagePack.Formatters.Intense.Master.TutorialStepMasterFormatter();
                case 11: return new MessagePack.Formatters.Intense.Master.TutorialMasterFormatter();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612


#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Formatters.Intense.Master
{
    using System;
    using System.Collections.Generic;
    using MessagePack;

    public sealed class BaseMasterFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Intense.Master.BaseMaster>
    {
        readonly Dictionary<RuntimeTypeHandle, KeyValuePair<int, int>> typeToKeyAndJumpMap;
        readonly Dictionary<int, int> keyToJumpMap;

        public BaseMasterFormatter()
        {
            this.typeToKeyAndJumpMap = new Dictionary<RuntimeTypeHandle, KeyValuePair<int, int>>(11, global::MessagePack.Internal.RuntimeTypeHandleEqualityComparer.Default)
            {
                { typeof(global::Intense.Master.VersionMaster).TypeHandle, new KeyValuePair<int, int>(0, 0) },
                { typeof(global::Intense.Master.TitleMaster).TypeHandle, new KeyValuePair<int, int>(1, 1) },
                { typeof(global::Intense.Master.SongSelectMaster).TypeHandle, new KeyValuePair<int, int>(2, 2) },
                { typeof(global::Intense.Master.SongMaster).TypeHandle, new KeyValuePair<int, int>(3, 3) },
                { typeof(global::Intense.Master.SongScoreRateMaster).TypeHandle, new KeyValuePair<int, int>(4, 4) },
                { typeof(global::Intense.Master.SongJudgeZoneMaster).TypeHandle, new KeyValuePair<int, int>(5, 5) },
                { typeof(global::Intense.Master.SongHpRateMaster).TypeHandle, new KeyValuePair<int, int>(6, 6) },
                { typeof(global::Intense.Master.ResultMaster).TypeHandle, new KeyValuePair<int, int>(7, 7) },
                { typeof(global::Intense.Master.SoundSheetNameMaster).TypeHandle, new KeyValuePair<int, int>(8, 8) },
                { typeof(global::Intense.Master.TutorialStepMaster).TypeHandle, new KeyValuePair<int, int>(9, 9) },
                { typeof(global::Intense.Master.TutorialMaster).TypeHandle, new KeyValuePair<int, int>(10, 10) },
            };
            this.keyToJumpMap = new Dictionary<int, int>(11)
            {
                { 0, 0 },
                { 1, 1 },
                { 2, 2 },
                { 3, 3 },
                { 4, 4 },
                { 5, 5 },
                { 6, 6 },
                { 7, 7 },
                { 8, 8 },
                { 9, 9 },
                { 10, 10 },
            };
        }

        public int Serialize(ref byte[] bytes, int offset, global::Intense.Master.BaseMaster value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            KeyValuePair<int, int> keyValuePair;
            if (value != null && this.typeToKeyAndJumpMap.TryGetValue(value.GetType().TypeHandle, out keyValuePair))
            {
                var startOffset = offset;
                offset += MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
                offset += MessagePackBinary.WriteInt32(ref bytes, offset, keyValuePair.Key);
                switch (keyValuePair.Value)
                {
                    case 0:
                        offset += formatterResolver.GetFormatterWithVerify<global::Intense.Master.VersionMaster>().Serialize(ref bytes, offset, (global::Intense.Master.VersionMaster)value, formatterResolver);
                        break;
                    case 1:
                        offset += formatterResolver.GetFormatterWithVerify<global::Intense.Master.TitleMaster>().Serialize(ref bytes, offset, (global::Intense.Master.TitleMaster)value, formatterResolver);
                        break;
                    case 2:
                        offset += formatterResolver.GetFormatterWithVerify<global::Intense.Master.SongSelectMaster>().Serialize(ref bytes, offset, (global::Intense.Master.SongSelectMaster)value, formatterResolver);
                        break;
                    case 3:
                        offset += formatterResolver.GetFormatterWithVerify<global::Intense.Master.SongMaster>().Serialize(ref bytes, offset, (global::Intense.Master.SongMaster)value, formatterResolver);
                        break;
                    case 4:
                        offset += formatterResolver.GetFormatterWithVerify<global::Intense.Master.SongScoreRateMaster>().Serialize(ref bytes, offset, (global::Intense.Master.SongScoreRateMaster)value, formatterResolver);
                        break;
                    case 5:
                        offset += formatterResolver.GetFormatterWithVerify<global::Intense.Master.SongJudgeZoneMaster>().Serialize(ref bytes, offset, (global::Intense.Master.SongJudgeZoneMaster)value, formatterResolver);
                        break;
                    case 6:
                        offset += formatterResolver.GetFormatterWithVerify<global::Intense.Master.SongHpRateMaster>().Serialize(ref bytes, offset, (global::Intense.Master.SongHpRateMaster)value, formatterResolver);
                        break;
                    case 7:
                        offset += formatterResolver.GetFormatterWithVerify<global::Intense.Master.ResultMaster>().Serialize(ref bytes, offset, (global::Intense.Master.ResultMaster)value, formatterResolver);
                        break;
                    case 8:
                        offset += formatterResolver.GetFormatterWithVerify<global::Intense.Master.SoundSheetNameMaster>().Serialize(ref bytes, offset, (global::Intense.Master.SoundSheetNameMaster)value, formatterResolver);
                        break;
                    case 9:
                        offset += formatterResolver.GetFormatterWithVerify<global::Intense.Master.TutorialStepMaster>().Serialize(ref bytes, offset, (global::Intense.Master.TutorialStepMaster)value, formatterResolver);
                        break;
                    case 10:
                        offset += formatterResolver.GetFormatterWithVerify<global::Intense.Master.TutorialMaster>().Serialize(ref bytes, offset, (global::Intense.Master.TutorialMaster)value, formatterResolver);
                        break;
                    default:
                        break;
                }

                return offset - startOffset;
            }

            return MessagePackBinary.WriteNil(ref bytes, offset);
        }
        
        public global::Intense.Master.BaseMaster Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            
            if (MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize) != 2)
            {
                throw new InvalidOperationException("Invalid Union data was detected. Type:global::Intense.Master.BaseMaster");
            }
            offset += readSize;

            var key = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
            offset += readSize;

            if (!this.keyToJumpMap.TryGetValue(key, out key))
            {
                key = -1;
            }

            global::Intense.Master.BaseMaster result = null;
            switch (key)
            {
                case 0:
                    result = (global::Intense.Master.BaseMaster)formatterResolver.GetFormatterWithVerify<global::Intense.Master.VersionMaster>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 1:
                    result = (global::Intense.Master.BaseMaster)formatterResolver.GetFormatterWithVerify<global::Intense.Master.TitleMaster>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 2:
                    result = (global::Intense.Master.BaseMaster)formatterResolver.GetFormatterWithVerify<global::Intense.Master.SongSelectMaster>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 3:
                    result = (global::Intense.Master.BaseMaster)formatterResolver.GetFormatterWithVerify<global::Intense.Master.SongMaster>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 4:
                    result = (global::Intense.Master.BaseMaster)formatterResolver.GetFormatterWithVerify<global::Intense.Master.SongScoreRateMaster>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 5:
                    result = (global::Intense.Master.BaseMaster)formatterResolver.GetFormatterWithVerify<global::Intense.Master.SongJudgeZoneMaster>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 6:
                    result = (global::Intense.Master.BaseMaster)formatterResolver.GetFormatterWithVerify<global::Intense.Master.SongHpRateMaster>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 7:
                    result = (global::Intense.Master.BaseMaster)formatterResolver.GetFormatterWithVerify<global::Intense.Master.ResultMaster>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 8:
                    result = (global::Intense.Master.BaseMaster)formatterResolver.GetFormatterWithVerify<global::Intense.Master.SoundSheetNameMaster>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 9:
                    result = (global::Intense.Master.BaseMaster)formatterResolver.GetFormatterWithVerify<global::Intense.Master.TutorialStepMaster>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 10:
                    result = (global::Intense.Master.BaseMaster)formatterResolver.GetFormatterWithVerify<global::Intense.Master.TutorialMaster>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                default:
                    offset += MessagePackBinary.ReadNextBlock(bytes, offset);
                    break;
            }
            
            readSize = offset - startOffset;
            
            return result;
        }
    }


}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Formatters.Intense.Master
{
    using System;
    using MessagePack;


    public sealed class VersionMasterFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Intense.Master.VersionMaster>
    {

        readonly global::MessagePack.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public VersionMasterFormatter()
        {
            this.____keyMapping = new global::MessagePack.Internal.AutomataDictionary()
            {
                { "Version", 0},
            };

            this.____stringByteKeys = new byte[][]
            {
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Version"),
                
            };
        }


        public int Serialize(ref byte[] bytes, int offset, global::Intense.Master.VersionMaster value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedMapHeaderUnsafe(ref bytes, offset, 1);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[0]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Version);
            return offset - startOffset;
        }

        public global::Intense.Master.VersionMaster Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadMapHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Version__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.MessagePackBinary.ReadStringSegment(bytes, offset, out readSize);
                offset += readSize;
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __Version__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                
                NEXT_LOOP:
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::Intense.Master.VersionMaster();
            ____result.Version = __Version__;
            return ____result;
        }
    }


    public sealed class TitleMasterFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Intense.Master.TitleMaster>
    {

        readonly global::MessagePack.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public TitleMasterFormatter()
        {
            this.____keyMapping = new global::MessagePack.Internal.AutomataDictionary()
            {
                { "Tid", 0},
            };

            this.____stringByteKeys = new byte[][]
            {
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Tid"),
                
            };
        }


        public int Serialize(ref byte[] bytes, int offset, global::Intense.Master.TitleMaster value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedMapHeaderUnsafe(ref bytes, offset, 1);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[0]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Tid);
            return offset - startOffset;
        }

        public global::Intense.Master.TitleMaster Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadMapHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Tid__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.MessagePackBinary.ReadStringSegment(bytes, offset, out readSize);
                offset += readSize;
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __Tid__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                
                NEXT_LOOP:
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::Intense.Master.TitleMaster();
            ____result.Tid = __Tid__;
            return ____result;
        }
    }


    public sealed class SongSelectMasterFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Intense.Master.SongSelectMaster>
    {

        readonly global::MessagePack.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public SongSelectMasterFormatter()
        {
            this.____keyMapping = new global::MessagePack.Internal.AutomataDictionary()
            {
                { "Group", 0},
                { "StartSongTime", 1},
                { "SongTime", 2},
            };

            this.____stringByteKeys = new byte[][]
            {
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Group"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("StartSongTime"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("SongTime"),
                
            };
        }


        public int Serialize(ref byte[] bytes, int offset, global::Intense.Master.SongSelectMaster value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedMapHeaderUnsafe(ref bytes, offset, 3);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[0]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Group);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[1]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.StartSongTime);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[2]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.SongTime);
            return offset - startOffset;
        }

        public global::Intense.Master.SongSelectMaster Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadMapHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Group__ = default(int);
            var __StartSongTime__ = default(int);
            var __SongTime__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.MessagePackBinary.ReadStringSegment(bytes, offset, out readSize);
                offset += readSize;
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __Group__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 1:
                        __StartSongTime__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __SongTime__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                
                NEXT_LOOP:
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::Intense.Master.SongSelectMaster();
            ____result.Group = __Group__;
            ____result.StartSongTime = __StartSongTime__;
            ____result.SongTime = __SongTime__;
            return ____result;
        }
    }


    public sealed class SongMasterFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Intense.Master.SongMaster>
    {

        readonly global::MessagePack.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public SongMasterFormatter()
        {
            this.____keyMapping = new global::MessagePack.Internal.AutomataDictionary()
            {
                { "Sid", 0},
                { "Group", 1},
                { "Difficulty", 2},
                { "Name", 3},
                { "Composer", 4},
                { "Start_offset", 5},
                { "Bg", 6},
                { "Score", 7},
                { "Hp", 8},
            };

            this.____stringByteKeys = new byte[][]
            {
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Sid"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Group"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Difficulty"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Name"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Composer"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Start_offset"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Bg"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Score"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Hp"),
                
            };
        }


        public int Serialize(ref byte[] bytes, int offset, global::Intense.Master.SongMaster value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedMapHeaderUnsafe(ref bytes, offset, 9);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[0]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Sid);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[1]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Group);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[2]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Difficulty);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[3]);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Name, formatterResolver);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[4]);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Composer, formatterResolver);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[5]);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.Start_offset);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[6]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Bg);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[7]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Score);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[8]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Hp);
            return offset - startOffset;
        }

        public global::Intense.Master.SongMaster Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadMapHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Sid__ = default(int);
            var __Group__ = default(int);
            var __Difficulty__ = default(int);
            var __Name__ = default(string);
            var __Composer__ = default(string);
            var __Start_offset__ = default(float);
            var __Bg__ = default(int);
            var __Score__ = default(int);
            var __Hp__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.MessagePackBinary.ReadStringSegment(bytes, offset, out readSize);
                offset += readSize;
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __Sid__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 1:
                        __Group__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __Difficulty__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 3:
                        __Name__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __Composer__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __Start_offset__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    case 6:
                        __Bg__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 7:
                        __Score__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 8:
                        __Hp__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                
                NEXT_LOOP:
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::Intense.Master.SongMaster();
            ____result.Sid = __Sid__;
            ____result.Group = __Group__;
            ____result.Difficulty = __Difficulty__;
            ____result.Name = __Name__;
            ____result.Composer = __Composer__;
            ____result.Start_offset = __Start_offset__;
            ____result.Bg = __Bg__;
            ____result.Score = __Score__;
            ____result.Hp = __Hp__;
            return ____result;
        }
    }


    public sealed class SongScoreRateMasterFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Intense.Master.SongScoreRateMaster>
    {

        readonly global::MessagePack.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public SongScoreRateMasterFormatter()
        {
            this.____keyMapping = new global::MessagePack.Internal.AutomataDictionary()
            {
                { "Type", 0},
                { "Rate", 1},
            };

            this.____stringByteKeys = new byte[][]
            {
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Type"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Rate"),
                
            };
        }


        public int Serialize(ref byte[] bytes, int offset, global::Intense.Master.SongScoreRateMaster value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedMapHeaderUnsafe(ref bytes, offset, 2);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[0]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Type);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[1]);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.Rate);
            return offset - startOffset;
        }

        public global::Intense.Master.SongScoreRateMaster Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadMapHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Type__ = default(int);
            var __Rate__ = default(float);

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.MessagePackBinary.ReadStringSegment(bytes, offset, out readSize);
                offset += readSize;
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __Type__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 1:
                        __Rate__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                
                NEXT_LOOP:
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::Intense.Master.SongScoreRateMaster();
            ____result.Type = __Type__;
            ____result.Rate = __Rate__;
            return ____result;
        }
    }


    public sealed class SongJudgeZoneMasterFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Intense.Master.SongJudgeZoneMaster>
    {

        readonly global::MessagePack.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public SongJudgeZoneMasterFormatter()
        {
            this.____keyMapping = new global::MessagePack.Internal.AutomataDictionary()
            {
                { "Type", 0},
                { "Zone", 1},
            };

            this.____stringByteKeys = new byte[][]
            {
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Type"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Zone"),
                
            };
        }


        public int Serialize(ref byte[] bytes, int offset, global::Intense.Master.SongJudgeZoneMaster value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedMapHeaderUnsafe(ref bytes, offset, 2);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[0]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Type);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[1]);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.Zone);
            return offset - startOffset;
        }

        public global::Intense.Master.SongJudgeZoneMaster Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadMapHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Type__ = default(int);
            var __Zone__ = default(float);

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.MessagePackBinary.ReadStringSegment(bytes, offset, out readSize);
                offset += readSize;
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __Type__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 1:
                        __Zone__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                
                NEXT_LOOP:
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::Intense.Master.SongJudgeZoneMaster();
            ____result.Type = __Type__;
            ____result.Zone = __Zone__;
            return ____result;
        }
    }


    public sealed class SongHpRateMasterFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Intense.Master.SongHpRateMaster>
    {

        readonly global::MessagePack.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public SongHpRateMasterFormatter()
        {
            this.____keyMapping = new global::MessagePack.Internal.AutomataDictionary()
            {
                { "Type", 0},
                { "Rate", 1},
            };

            this.____stringByteKeys = new byte[][]
            {
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Type"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Rate"),
                
            };
        }


        public int Serialize(ref byte[] bytes, int offset, global::Intense.Master.SongHpRateMaster value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedMapHeaderUnsafe(ref bytes, offset, 2);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[0]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Type);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[1]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Rate);
            return offset - startOffset;
        }

        public global::Intense.Master.SongHpRateMaster Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadMapHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Type__ = default(int);
            var __Rate__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.MessagePackBinary.ReadStringSegment(bytes, offset, out readSize);
                offset += readSize;
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __Type__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 1:
                        __Rate__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                
                NEXT_LOOP:
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::Intense.Master.SongHpRateMaster();
            ____result.Type = __Type__;
            ____result.Rate = __Rate__;
            return ____result;
        }
    }


    public sealed class ResultMasterFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Intense.Master.ResultMaster>
    {

        readonly global::MessagePack.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public ResultMasterFormatter()
        {
            this.____keyMapping = new global::MessagePack.Internal.AutomataDictionary()
            {
                { "Rid", 0},
            };

            this.____stringByteKeys = new byte[][]
            {
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Rid"),
                
            };
        }


        public int Serialize(ref byte[] bytes, int offset, global::Intense.Master.ResultMaster value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedMapHeaderUnsafe(ref bytes, offset, 1);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[0]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Rid);
            return offset - startOffset;
        }

        public global::Intense.Master.ResultMaster Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadMapHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Rid__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.MessagePackBinary.ReadStringSegment(bytes, offset, out readSize);
                offset += readSize;
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __Rid__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                
                NEXT_LOOP:
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::Intense.Master.ResultMaster();
            ____result.Rid = __Rid__;
            return ____result;
        }
    }


    public sealed class SoundSheetNameMasterFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Intense.Master.SoundSheetNameMaster>
    {

        readonly global::MessagePack.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public SoundSheetNameMasterFormatter()
        {
            this.____keyMapping = new global::MessagePack.Internal.AutomataDictionary()
            {
                { "Category", 0},
                { "Id", 1},
                { "SheetName", 2},
                { "CueName", 3},
            };

            this.____stringByteKeys = new byte[][]
            {
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Category"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Id"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("SheetName"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("CueName"),
                
            };
        }


        public int Serialize(ref byte[] bytes, int offset, global::Intense.Master.SoundSheetNameMaster value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedMapHeaderUnsafe(ref bytes, offset, 4);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[0]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Category);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[1]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Id);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[2]);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.SheetName, formatterResolver);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[3]);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.CueName, formatterResolver);
            return offset - startOffset;
        }

        public global::Intense.Master.SoundSheetNameMaster Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadMapHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Category__ = default(int);
            var __Id__ = default(int);
            var __SheetName__ = default(string);
            var __CueName__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.MessagePackBinary.ReadStringSegment(bytes, offset, out readSize);
                offset += readSize;
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __Category__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 1:
                        __Id__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __SheetName__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __CueName__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                
                NEXT_LOOP:
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::Intense.Master.SoundSheetNameMaster();
            ____result.Category = __Category__;
            ____result.Id = __Id__;
            ____result.SheetName = __SheetName__;
            ____result.CueName = __CueName__;
            return ____result;
        }
    }


    public sealed class TutorialStepMasterFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Intense.Master.TutorialStepMaster>
    {

        readonly global::MessagePack.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public TutorialStepMasterFormatter()
        {
            this.____keyMapping = new global::MessagePack.Internal.AutomataDictionary()
            {
                { "Tid", 0},
                { "StepOrder", 1},
                { "Description", 2},
                { "TriggerTime", 3},
                { "HintPositionX", 4},
                { "HintPositionY", 5},
                { "IsSkippable", 6},
            };

            this.____stringByteKeys = new byte[][]
            {
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Tid"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("StepOrder"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Description"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("TriggerTime"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("HintPositionX"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("HintPositionY"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("IsSkippable"),
                
            };
        }


        public int Serialize(ref byte[] bytes, int offset, global::Intense.Master.TutorialStepMaster value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedMapHeaderUnsafe(ref bytes, offset, 7);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[0]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Tid);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[1]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.StepOrder);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[2]);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Description, formatterResolver);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[3]);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.TriggerTime);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[4]);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.HintPositionX);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[5]);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.HintPositionY);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[6]);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.IsSkippable);
            return offset - startOffset;
        }

        public global::Intense.Master.TutorialStepMaster Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadMapHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Tid__ = default(int);
            var __StepOrder__ = default(int);
            var __Description__ = default(string);
            var __TriggerTime__ = default(float);
            var __HintPositionX__ = default(float);
            var __HintPositionY__ = default(float);
            var __IsSkippable__ = default(bool);

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.MessagePackBinary.ReadStringSegment(bytes, offset, out readSize);
                offset += readSize;
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __Tid__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 1:
                        __StepOrder__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __Description__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __TriggerTime__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    case 4:
                        __HintPositionX__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    case 5:
                        __HintPositionY__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    case 6:
                        __IsSkippable__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                
                NEXT_LOOP:
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::Intense.Master.TutorialStepMaster();
            ____result.Tid = __Tid__;
            ____result.StepOrder = __StepOrder__;
            ____result.Description = __Description__;
            ____result.TriggerTime = __TriggerTime__;
            ____result.HintPositionX = __HintPositionX__;
            ____result.HintPositionY = __HintPositionY__;
            ____result.IsSkippable = __IsSkippable__;
            return ____result;
        }
    }


    public sealed class TutorialMasterFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Intense.Master.TutorialMaster>
    {

        readonly global::MessagePack.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public TutorialMasterFormatter()
        {
            this.____keyMapping = new global::MessagePack.Internal.AutomataDictionary()
            {
                { "Tid", 0},
                { "Title", 1},
                { "Description", 2},
                { "Sid", 3},
                { "Order", 4},
                { "IsRequired", 5},
                { "VoiceCue", 6},
                { "HintText", 7},
                { "Type", 8},
            };

            this.____stringByteKeys = new byte[][]
            {
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Tid"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Title"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Description"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Sid"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Order"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("IsRequired"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("VoiceCue"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("HintText"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("Type"),
                
            };
        }


        public int Serialize(ref byte[] bytes, int offset, global::Intense.Master.TutorialMaster value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedMapHeaderUnsafe(ref bytes, offset, 9);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[0]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Tid);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[1]);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Title, formatterResolver);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[2]);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Description, formatterResolver);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[3]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Sid);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[4]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Order);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[5]);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.IsRequired);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[6]);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.VoiceCue, formatterResolver);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[7]);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.HintText, formatterResolver);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[8]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Type);
            return offset - startOffset;
        }

        public global::Intense.Master.TutorialMaster Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadMapHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Tid__ = default(int);
            var __Title__ = default(string);
            var __Description__ = default(string);
            var __Sid__ = default(int);
            var __Order__ = default(int);
            var __IsRequired__ = default(bool);
            var __VoiceCue__ = default(string);
            var __HintText__ = default(string);
            var __Type__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.MessagePackBinary.ReadStringSegment(bytes, offset, out readSize);
                offset += readSize;
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __Tid__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 1:
                        __Title__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Description__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Sid__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 4:
                        __Order__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 5:
                        __IsRequired__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 6:
                        __VoiceCue__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __HintText__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 8:
                        __Type__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                
                NEXT_LOOP:
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::Intense.Master.TutorialMaster();
            ____result.Tid = __Tid__;
            ____result.Title = __Title__;
            ____result.Description = __Description__;
            ____result.Sid = __Sid__;
            ____result.Order = __Order__;
            ____result.IsRequired = __IsRequired__;
            ____result.VoiceCue = __VoiceCue__;
            ____result.HintText = __HintText__;
            ____result.Type = __Type__;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612