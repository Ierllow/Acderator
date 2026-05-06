using System;

namespace uPalette.Generated
{
public enum ColorTheme
    {
        Default,
    }

    public static class ColorThemeExtensions
    {
        public static string ToThemeId(this ColorTheme theme) => theme switch
        {
            ColorTheme.Default => "4a669972-2591-4c38-a9d2-e39bbd662904",
            _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null),
        };
    }

    public enum ColorEntry
    {
        LightWhite,
        White,
    }

    public static class ColorEntryExtensions
    {
        public static string ToEntryId(this ColorEntry entry) => entry switch
        {
            ColorEntry.LightWhite => "961a5454-79e6-4eac-9b64-403cc4fa557b",
            ColorEntry.White => "7774e83a-9894-4521-b48c-378821c933ff",
            _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, null),
        };
    }

    public enum GradientTheme
    {
        Default,
    }

    public static class GradientThemeExtensions
    {
        public static string ToThemeId(this GradientTheme theme) => theme switch
        {
            GradientTheme.Default => "69c39b37-a72c-4ea3-b33d-46a623c2fa6c",
            _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null),
        };
    }

    public enum GradientEntry
    {
        Exc,
        Clear,
        Failed,
    }

    public static class GradientEntryExtensions
    {
        public static string ToEntryId(this GradientEntry entry) => entry switch
        {
            GradientEntry.Exc => "8ca02421-58ec-47b3-8cf7-89e70d3bc65d",
            GradientEntry.Clear => "fc650475-e8f8-4ec7-91fb-d81e7e97934f",
            GradientEntry.Failed => "ed740ee4-c50c-4a12-b62c-9bca9b158a16",
            _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, null),
        };
    }

    public enum CharacterStyleTheme
    {
        Default,
    }

    public static class CharacterStyleThemeExtensions
    {
        public static string ToThemeId(this CharacterStyleTheme theme) => theme switch
        {
            CharacterStyleTheme.Default => "37dfcba0-1f0d-43e6-9bc8-290c48fd17b5",
            _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null),
        };
    }

    public enum CharacterStyleEntry
    {
    }

    public static class CharacterStyleEntryExtensions
    {
        public static string ToEntryId(this CharacterStyleEntry entry) => entry switch
        {
            _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, null),
        };
    }

    public enum CharacterStyleTMPTheme
    {
        Default,
    }

    public static class CharacterStyleTMPThemeExtensions
    {
        public static string ToThemeId(this CharacterStyleTMPTheme theme) => theme switch
        {
            CharacterStyleTMPTheme.Default => "a357cca1-a77e-4e3d-b592-06ebcf0da609",
            _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null),
        };
    }

    public enum CharacterStyleTMPEntry
    {
    }

    public static class CharacterStyleTMPEntryExtensions
    {
        public static string ToEntryId(this CharacterStyleTMPEntry entry) => entry switch
        {
            _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, null),
        };
    }
}