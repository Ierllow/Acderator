using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class CodeGenerators
{
        private const string MasterMemoryGeneratorVersion = "1.3.1";
        private const string MessagePackGeneratorVersion = "2.0.323";

        public static void GenerateAll()
        {
                SpreadsheetToMasterGenerator.Generate();
                ExecuteMasterMemoryCodeGenerator();
                ExecuteMessagePackCodeGenerator();
        }

        public static void ExecuteMasterMemoryCodeGenerator()
        {
                UnityEngine.Debug.Log("start ExecuteMasterMemoryCodeGenerator");

                var exProcess = new Process();

                var rootPath = Application.dataPath + "/..";
                var generatorPath = $"{rootPath}/Packages/MasterMemory.Generator.{MasterMemoryGeneratorVersion}/tools/netcoreapp2.2/any/MasterMemory.Generator.dll";

                var psi = new ProcessStartInfo()
                {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        FileName = "dotnet",
                        Arguments = $@"exec ""{generatorPath}"" -i ""{Application.dataPath}/Acderator/Scripts/Intense"" -o ""{Application.dataPath}/Acderator/Scripts/Intense/Master"" -n ""Master""",
                        WorkingDirectory = rootPath
                };
                psi.EnvironmentVariables["DOTNET_ROLL_FORWARD"] = "Major";

                var p = Process.Start(psi);

                p.EnableRaisingEvents = true;
                p.Exited += (sender, e) =>
                {
                        var data = p.StandardOutput.ReadToEnd();
                        UnityEngine.Debug.Log(string.Format("{0}", data));
                        UnityEngine.Debug.Log("end ExecuteMasterMemoryCodeGenerator");
                        p.Dispose();
                        p = null;
                };
        }

        public static void ExecuteMessagePackCodeGenerator()
        {
                UnityEngine.Debug.Log("start ExecuteMessagePackCodeGenerator");

                var rootPath = Application.dataPath + "/..";
                var generatorPath = $"{rootPath}/Packages/MessagePack.Generator.{MessagePackGeneratorVersion}/tools/netcoreapp3.0/any/mpc.dll";
                var input = $"{Application.dataPath}/Acderator/Scripts/Intense";

                var outputDir = $"{Application.dataPath}/Acderator/Scripts/Intense/Generated";
                var output = $"{outputDir}/MessagePackGenerated.cs";

                if (!Directory.Exists(outputDir))
                {
                        Directory.CreateDirectory(outputDir);
                }

                var psi = new ProcessStartInfo()
                {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        FileName = "dotnet",
                        Arguments = $@"exec ""{generatorPath}"" -i ""{input}"" -o ""{output}""",
                        WorkingDirectory = rootPath
                };
                psi.EnvironmentVariables["DOTNET_ROLL_FORWARD"] = "Major";

                using var p = Process.Start(psi);
                var stdout = p.StandardOutput.ReadToEnd();
                var stderr = p.StandardError.ReadToEnd();
                p.WaitForExit();

                UnityEngine.Debug.Log($"exe: dotnet exec {generatorPath}");
                UnityEngine.Debug.Log($"input: {input}");
                UnityEngine.Debug.Log($"output: {output}");
                UnityEngine.Debug.Log($"ExitCode: {p.ExitCode}");

                if (!string.IsNullOrEmpty(stdout)) UnityEngine.Debug.Log(stdout);
                if (!string.IsNullOrEmpty(stderr)) UnityEngine.Debug.LogError(stderr);

                PatchMessagePackGeneratedCode(output);
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log("end ExecuteMessagePackCodeGenerator");
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