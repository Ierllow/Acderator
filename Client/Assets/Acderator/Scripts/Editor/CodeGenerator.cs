using Cysharp.Threading.Tasks;
using Debug = UnityEngine.Debug;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class CodeGenerator
{
    private const string MasterMemoryGeneratorVersion = "1.3.1";
    private const string MessagePackGeneratorVersion = "2.0.323";

    public static void GenerateAll() => UniTask.Void(async () =>
    {
        await SpreadsheetToMasterGenerator.GenerateAsync();
        if (!GenerateMasterMemory()) return;
        GenerateMessagePack();
    });

    public static bool GenerateMasterMemory()
    {
        var rootPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        var generatorPath = Path.Combine(
            rootPath,
            "Packages",
            $"MasterMemory.Generator.{MasterMemoryGeneratorVersion}",
            "tools",
            "netcoreapp2.2",
            "any",
            "MasterMemory.Generator.dll");
        var arguments = $@"exec ""{generatorPath}"" -i ""{Application.dataPath}/Acderator/Scripts/Intense"" -o ""{Application.dataPath}/Acderator/Scripts/Intense/Master"" -n ""Master""";
        return RunGenerator("MasterMemory", rootPath, arguments);
    }

    public static bool GenerateMessagePack()
    {
        var rootPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        var generatorPath = Path.Combine(
            rootPath,
            "Packages",
            $"MessagePack.Generator.{MessagePackGeneratorVersion}",
            "tools",
            "netcoreapp3.0",
            "any",
            "mpc.dll");
        var input = $"{Application.dataPath}/Acderator/Scripts/Intense";
        var outputDirectory = $"{Application.dataPath}/Acderator/Scripts/Intense/Generated";
        var output = $"{outputDirectory}/MessagePackGenerated.cs";

        Directory.CreateDirectory(outputDirectory);
        var arguments = $@"exec ""{generatorPath}"" -i ""{input}"" -o ""{output}""";
        if (!RunGenerator("MessagePack", rootPath, arguments)) return false;

        PatchMessagePackGeneratedCode(output);
        AssetDatabase.Refresh();
        return true;
    }

    private static bool RunGenerator(string generatorName, string workingDirectory, string arguments)
    {
        Debug.Log($"Starting {generatorName} code generation.");
        var startInfo = new ProcessStartInfo
        {
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            FileName = "dotnet",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
        };
        startInfo.EnvironmentVariables["DOTNET_ROLL_FORWARD"] = "Major";

        using var process = Process.Start(startInfo);
        if (process == null) throw new InvalidOperationException($"Could not start {generatorName} code generator.");

        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        var output = standardOutput.GetAwaiter().GetResult();
        var error = standardError.GetAwaiter().GetResult();

        if (!string.IsNullOrEmpty(output)) Debug.Log(output);
        if (!string.IsNullOrEmpty(error)) Debug.LogError(error);

        var succeeded = process.ExitCode == 0;
        Debug.Log($"{generatorName} code generation finished with exit code {process.ExitCode}.");
        return succeeded;
    }

    private static void PatchMessagePackGeneratedCode(string output)
    {
        var source = File.ReadAllText(output);
        source = Regex.Replace(
            source,
            @"writer\.WriteRaw\(this\.____stringByteKeys\[(\d+)\]\);",
            "writer.WriteRaw(new ReadOnlySequence<byte>(this.____stringByteKeys[$1]));");
        source = source.Replace(
            "ReadOnlySpan<byte> stringKey = Internal.CodeGenHelpers.ReadStringSpan(ref reader);",
            "ReadOnlySequence<byte> stringKey = reader.ReadStringSequence().Value;");
        File.WriteAllText(output, source);
    }
}