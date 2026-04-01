using System;
using System.Collections.Generic;
using UnityEngine;

internal enum EKey
{
    UserName,
    UserId,
    PassWard,
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
    Token,
}

internal static class PlayerPrefsValues
{
    private static readonly Dictionary<EKey, string> PlayerPrefsDict = new()
    {
        { EKey.UserName, UserName },
        { EKey.UserId, UserId},
        { EKey.PassWard, PassWard },
        { EKey.BgmVolume, BgmVolume },
        { EKey.SeVolume, SeVolume },
        { EKey.SongVolume, SongVolume },
        { EKey.BgmMute, BgmMute },
        { EKey.SeMute, SeMute },
        { EKey.SongMute, SongMute },
        { EKey.NoteSpeedConfig, NoteSpeedConfig },
        { EKey.TapTimingNum, TapTimingNum },
        { EKey.SelectedGroup, SelectedGroup },
        { EKey.SelectedDifficulty, SelectedDifficulty },
        { EKey.OrderType, OrderType },
    };

    private static readonly string UserName = "UserName";
    private static readonly string UserId = "UserId";
    private static readonly string PassWard = "PassWard";
    private static readonly string BgmVolume = "bgmVolume";
    private static readonly string SeVolume = "seVolume";
    private static readonly string SongVolume = "songVolume";
    private static readonly string BgmMute = "bgmMute";
    private static readonly string SeMute = "seMute";
    private static readonly string SongMute = "songMute";
    private static readonly string NoteSpeedConfig = "noteSpeedConfig";
    private static readonly string TapTimingNum = "tapTimingNum";
    private static readonly string SelectedGroup = "SelectedGroup";
    private static readonly string SelectedDifficulty = "SelectedDifficulty";
    private static readonly string OrderType = "OrderType";

    public static readonly string UN = PlayerPrefs.GetString(UserName, "");
    public static readonly string UI = PlayerPrefs.GetString(UserId, "");
    public static readonly string PW = PlayerPrefs.GetString(PassWard, "");
    public static readonly float BV = PlayerPrefs.GetFloat(BgmVolume, 1f);
    public static readonly float SV = PlayerPrefs.GetFloat(SeVolume, 1f);
    public static readonly float SGV = PlayerPrefs.GetFloat(SongVolume, 1f);
    public static readonly bool BM = PlayerPrefs.HasKey(BgmMute) && PlayerPrefs.GetInt(BgmMute) == 1;
    public static readonly bool SM = PlayerPrefs.HasKey(SeMute) && PlayerPrefs.GetInt(SeMute) == 1;
    public static readonly bool SOM = PlayerPrefs.HasKey(SongMute) && PlayerPrefs.GetInt(SongMute) == 1;
    public static readonly float NS = PlayerPrefs.GetFloat(NoteSpeedConfig, 1f);
    public static readonly float TN = PlayerPrefs.GetFloat(TapTimingNum, 0f);
    public static readonly int SDG = PlayerPrefs.GetInt(SelectedGroup, 1);
    public static readonly int SDD = PlayerPrefs.GetInt(SelectedDifficulty, 1);
    public static readonly int OT = PlayerPrefs.GetInt(OrderType, 0);
    internal static string TK { get; set; }

    public static void Set<T>(EKey key, T value)
    {
        if (typeof(T) == typeof(T)) PlayerPrefs.SetInt(PlayerPrefsDict[key], Convert.ToInt32(value));
        else if (typeof(T) == typeof(string)) PlayerPrefs.SetString(PlayerPrefsDict[key], Convert.ToString(value));
        else if (typeof(T) == typeof(float)) PlayerPrefs.SetFloat(PlayerPrefsDict[key], Convert.ToSingle(value));
        return;
    }
}