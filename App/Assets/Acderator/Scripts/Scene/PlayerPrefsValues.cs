using System;
using UnityEngine;

internal enum PlayerPrefsKey
{
    UserName,
    UserId,
    Password,
    BgmVolume,
    SeVolume,
    SongVolume,
    BgmMute,
    SeMute,
    SongMute,
    NoteSpeedConfig,
    TapTimingNum,
    SelectedGroup,
    SelectedDifficulty,
    OrderType,
}

internal static class PlayerPrefsValues
{
    private const string UserNameKey = "UserName";
    private const string UserIdKey = "UserId";
    private const string PasswordKey = "PassWard";
    private const string BgmVolumeKey = "bgmVolume";
    private const string SeVolumeKey = "seVolume";
    private const string SongVolumeKey = "songVolume";
    private const string BgmMuteKey = "bgmMute";
    private const string SeMuteKey = "seMute";
    private const string SongMuteKey = "songMute";
    private const string NoteSpeedConfigKey = "noteSpeedConfig";
    private const string TapTimingNumKey = "tapTimingNum";
    private const string SelectedGroupKey = "SelectedGroup";
    private const string SelectedDifficultyKey = "SelectedDifficulty";
    private const string OrderTypeKey = "OrderType";

    public static string UserName => PlayerPrefs.GetString(UserNameKey, string.Empty);
    public static string UserId => PlayerPrefs.GetString(UserIdKey, string.Empty);
    public static string Password => PlayerPrefs.GetString(PasswordKey, string.Empty);
    public static float BgmVolume => PlayerPrefs.GetFloat(BgmVolumeKey, 1f);
    public static float SeVolume => PlayerPrefs.GetFloat(SeVolumeKey, 1f);
    public static float SongVolume => PlayerPrefs.GetFloat(SongVolumeKey, 1f);
    public static bool IsBgmMuted => GetBool(BgmMuteKey);
    public static bool IsSeMuted => GetBool(SeMuteKey);
    public static bool IsSongMuted => GetBool(SongMuteKey);
    public static float NoteSpeed => PlayerPrefs.GetFloat(NoteSpeedConfigKey, 1f);
    public static float TapTiming => PlayerPrefs.GetFloat(TapTimingNumKey, 0f);
    public static int SelectedGroup => PlayerPrefs.GetInt(SelectedGroupKey, 1);
    public static int SelectedDifficulty => PlayerPrefs.GetInt(SelectedDifficultyKey, 1);
    public static int OrderType => PlayerPrefs.GetInt(OrderTypeKey, 0);

    public static void Set(PlayerPrefsKey key, int value) => PlayerPrefs.SetInt(GetKey(key), value);

    public static void Set(PlayerPrefsKey key, float value) => PlayerPrefs.SetFloat(GetKey(key), value);

    public static void Set(PlayerPrefsKey key, string value) => PlayerPrefs.SetString(GetKey(key), value);

    public static void Set(PlayerPrefsKey key, bool value) => PlayerPrefs.SetInt(GetKey(key), value ? 1 : 0);

    private static bool GetBool(string key) => PlayerPrefs.GetInt(key, 0) == 1;

    private static string GetKey(PlayerPrefsKey key) => key switch
    {
        PlayerPrefsKey.UserName => UserNameKey,
        PlayerPrefsKey.UserId => UserIdKey,
        PlayerPrefsKey.Password => PasswordKey,
        PlayerPrefsKey.BgmVolume => BgmVolumeKey,
        PlayerPrefsKey.SeVolume => SeVolumeKey,
        PlayerPrefsKey.SongVolume => SongVolumeKey,
        PlayerPrefsKey.BgmMute => BgmMuteKey,
        PlayerPrefsKey.SeMute => SeMuteKey,
        PlayerPrefsKey.SongMute => SongMuteKey,
        PlayerPrefsKey.NoteSpeedConfig => NoteSpeedConfigKey,
        PlayerPrefsKey.TapTimingNum => TapTimingNumKey,
        PlayerPrefsKey.SelectedGroup => SelectedGroupKey,
        PlayerPrefsKey.SelectedDifficulty => SelectedDifficultyKey,
        PlayerPrefsKey.OrderType => OrderTypeKey,
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, null),
    };
}