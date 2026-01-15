#if UNITY_EDITOR
using Intense;
using Intense.Master;
using Master;
using MessagePack.Resolvers;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public enum EDifficulty
{
    Easy = 1,
    Normal,
    Expert,
}

public class MasterDataExporter
{
    private static TitleMaster[] TitleMasters => new TitleMaster[]
    {
        new() { Tid = 1, } ,
    };

    private static SongSelectMaster[] SongSelectMasters => new SongSelectMaster[]
    {
        new() { Group = 1, StartSongTime = 40000, SongTime = 10, } ,
    };

    private static SongMaster[] SongMasters => new SongMaster[]
    {
        new() { Sid = 1, Group = 1, Difficulty = EDifficulty.Easy.GetLength() , Name = "405nm", Composer = "Ryu��mix", Start_offset = 12.0f, Bg = 1 },
        new() { Sid = 2, Group = 1, Difficulty = EDifficulty.Normal.GetLength(), Name = "405nm", Composer = "Ryu��mix", Start_offset = 12.0f, Bg = 1 },
        new() { Sid = 3, Group = 1, Difficulty = EDifficulty.Expert.GetLength(), Name = "405nm", Composer = "Ryu��mix", Start_offset = 12.0f, Bg = 1 },
    };

    private static SongScoreRateMaster[] SongScoreRateMasters => new SongScoreRateMaster[]
    {
        new() { Type = 1, Rate = 1.0f, },
        new() { Type = 2, Rate = 0.7f, },
        new() { Type = 3, Rate = 0.4f, },
        new() { Type = 4, Rate = 0.1f, },
        new() { Type = 5, Rate = 0.0f, },
    };

    private static SongBaseScoreMaster[] SongBaseScoreMasters => new[]
    {
        new SongBaseScoreMaster { Score = 1000000, } ,
    };

    private static SongJudgeZoneMaster[] SongJudgeZoneMasters => new SongJudgeZoneMaster[]
    {
        new() { Type = EJudgementType.Perfect.GetLength(), Zone = 0.05f },
        new() { Type = EJudgementType.Great.GetLength(), Zone = 0.10f },
        new() { Type = EJudgementType.Good.GetLength(), Zone = 0.20f },
        new() { Type = EJudgementType.Bad.GetLength(), Zone = 0.30f },
        new() { Type = EJudgementType.Miss.GetLength(), Zone = 0.00f },
    };

    private static SongBaseHpMaster[] SongBaseHpMasters => new SongBaseHpMaster[]
    {
        new() { Hp = 70 },
    };

    private static SongHpRateMaster[] SongHpRateMasters => new SongHpRateMaster[]
    {
        new() { Type = EJudgementType.Perfect.GetLength(), Rate = 2048 },
        new() { Type = EJudgementType.Great.GetLength(), Rate = 2048 },
        new() { Type = EJudgementType.Good.GetLength(), Rate = 1024 },
        new() { Type = EJudgementType.Bad.GetLength(), Rate = 8192 },
        new() { Type = EJudgementType.Miss.GetLength(), Rate = 8192 },
    };

    private static SoundSheetNameMaster[] SoundSheetNameMasters => new SoundSheetNameMaster[]
    {
        new() { Category = ESoundCategory.Bgm.GetLength(), Id = 2, SheetName = "bgm", CueName = "result" },
        new() { Category = ESoundCategory.Bgm.GetLength(), Id = 3, SheetName = "bgm", CueName = "result_failed" },
        new() { Category = ESoundCategory.Song.GetLength(), Id = 0, SheetName = "music", CueName = "" },
        new() { Category = ESoundCategory.Se.GetLength(), Id = 1, SheetName = "music_se", CueName = "tap" },
        new() { Category = ESoundCategory.Se.GetLength(), Id = 2, SheetName = "music_se", CueName = "flick" },
    };

    private static ResultMaster[] ResultMasters => new ResultMaster[]
    {
        new() { Rid = 1, },
    };

    [MenuItem("Tools/Master/Export MasterData")]
    private static void Build()
    {
        try
        {
            CompositeResolver.RegisterAndSetAsDefault(new[] { MasterMemoryResolver.Instance, GeneratedResolver.Instance, StandardResolver.Instance });
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }

        GenerateMasterData();

        AssetDatabase.Refresh();
    }

    private static void GenerateMasterData()
    {
        const string path = "Assets/Acderator/MasterData/Export/master.bytes";

        var builder = new DatabaseBuilder();
        builder.Append(SongSelectMasters);
        builder.Append(SongMasters);
        builder.Append(SongScoreRateMasters);
        builder.Append(SongBaseScoreMasters);
        builder.Append(SongJudgeZoneMasters);
        builder.Append(SongBaseHpMasters);
        builder.Append(SongHpRateMasters);
        builder.Append(ResultMasters);
        builder.Append(SoundSheetNameMasters);
        Save(path, builder.Build());
    }

    private static void Save(string path, byte[] databaseBinary)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));

        using var stream = new FileStream(path, FileMode.Create);
        stream.Write(databaseBinary, 0, databaseBinary.Length);
    }
}
#endif