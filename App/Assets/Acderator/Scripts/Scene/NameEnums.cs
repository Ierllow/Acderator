using System;

namespace uPalette.Generated
{
public enum ColorTheme
    {
        Default,
    }

    public static class ColorThemeExtensions
    {
        public static string ToThemeId(this ColorTheme theme)
        {
            switch (theme)
            {
                case ColorTheme.Default:
                    return "4a669972-2591-4c38-a9d2-e39bbd662904";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum ColorEntry
    {
        LightWhite,
        White,
    }

    public static class ColorEntryExtensions
    {
        public static string ToEntryId(this ColorEntry entry)
        {
            switch (entry)
            {
                case ColorEntry.LightWhite:
                    return "961a5454-79e6-4eac-9b64-403cc4fa557b";
                case ColorEntry.White:
                    return "7774e83a-9894-4521-b48c-378821c933ff";
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }

    public enum GradientTheme
    {
        Default,
    }

    public static class GradientThemeExtensions
    {
        public static string ToThemeId(this GradientTheme theme)
        {
            switch (theme)
            {
                case GradientTheme.Default:
                    return "69c39b37-a72c-4ea3-b33d-46a623c2fa6c";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum GradientEntry
    {
        Exc,
        Clear,
        Failed,
    }

    public static class GradientEntryExtensions
    {
        public static string ToEntryId(this GradientEntry entry)
        {
            switch (entry)
            {
                case GradientEntry.Exc:
                    return "8ca02421-58ec-47b3-8cf7-89e70d3bc65d";
                case GradientEntry.Clear:
                    return "fc650475-e8f8-4ec7-91fb-d81e7e97934f";
                case GradientEntry.Failed:
                    return "ed740ee4-c50c-4a12-b62c-9bca9b158a16";
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }

    public enum CharacterStyleTheme
    {
        Default,
    }

    public static class CharacterStyleThemeExtensions
    {
        public static string ToThemeId(this CharacterStyleTheme theme)
        {
            switch (theme)
            {
                case CharacterStyleTheme.Default:
                    return "37dfcba0-1f0d-43e6-9bc8-290c48fd17b5";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum CharacterStyleEntry
    {
    }

    public static class CharacterStyleEntryExtensions
    {
        public static string ToEntryId(this CharacterStyleEntry entry)
        {
            switch (entry)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }

    public enum CharacterStyleTMPTheme
    {
        Default,
    }

    public static class CharacterStyleTMPThemeExtensions
    {
        public static string ToThemeId(this CharacterStyleTMPTheme theme)
        {
            switch (theme)
            {
                case CharacterStyleTMPTheme.Default:
                    return "a357cca1-a77e-4e3d-b592-06ebcf0da609";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum CharacterStyleTMPEntry
    {
    }

    public static class CharacterStyleTMPEntryExtensions
    {
        public static string ToEntryId(this CharacterStyleTMPEntry entry)
        {
            switch (entry)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }
}
