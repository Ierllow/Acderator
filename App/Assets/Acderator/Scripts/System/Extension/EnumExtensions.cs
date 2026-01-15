using FastEnumUtility;

namespace System
{
    public static class EnumExtensions
    {
        public static int GetLength<T>(this T value) where T : struct, Enum => value.ToInt32();
        public static bool EnumEquals<T1, T2>(this T1 value, T2 value2) where T1 : struct, Enum where T2 : struct, Enum => typeof(T1) == typeof(T2) && value.GetLength() == value2.GetLength();
    }
}