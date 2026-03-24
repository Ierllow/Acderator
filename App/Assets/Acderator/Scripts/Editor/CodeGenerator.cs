#if UNITY_EDITOR
using Cysharp.Text;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class CodeGenerators
{
        [MenuItem("Tools/MasterMemory/CodeGenerate")]
        private static void Generate()
        {
                ExecuteMasterMemoryCodeGenerator();
                ExecuteMessagePackCodeGenerator();
        }

        private static void ExecuteMasterMemoryCodeGenerator()
        {
                UnityEngine.Debug.Log("start ExecuteMasterMemoryCodeGenerator");

                var exProcess = new Process();

                var rootPath = Application.dataPath + "/..";
                var filePath = rootPath + "/GeneratorTools/MasterMemory.Generator";
                var exeFileName = "";
#if UNITY_EDITOR_WIN
                exeFileName = "/win-x64/MasterMemory.Generator.exe";
#elif UNITY_EDITOR_OSX
        exeFileName = "/osx-x64/MasterMemory.Generator";
#elif UNITY_EDITOR_LINUX
        exeFileName = "/linux-x64/MasterMemory.Generator";
#else
        return;
#endif

                var psi = new ProcessStartInfo()
                {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        FileName = filePath + exeFileName,
                        Arguments = $@"-i ""{Application.dataPath}/Acderator/Scripts/Intense"" -o ""{Application.dataPath}/Acderator/Scripts/Intense/Master"" -n ""Master""",
                };

                var p = Process.Start(psi);

                p.EnableRaisingEvents = true;
                p.Exited += (sender, e) =>
                {
                        var data = p.StandardOutput.ReadToEnd();
                        UnityEngine.Debug.Log(ZString.Format("{0}", data));
                        UnityEngine.Debug.Log("end ExecuteMasterMemoryCodeGenerator");
                        p.Dispose();
                        p = null;
                };
        }

        private static void ExecuteMessagePackCodeGenerator()
        {
                UnityEngine.Debug.Log("start ExecuteMessagePackCodeGenerator");

                var rootPath = Application.dataPath + "/..";
                var filePath = rootPath + "/GeneratorTools/MessagePackUniversalCodeGenerator";
                var exeFileName = "";
#if UNITY_EDITOR_WIN
                exeFileName = "/win-x64/mpc.exe";
#elif UNITY_EDITOR_OSX
        exeFileName = "/osx-x64/mpc";
#elif UNITY_EDITOR_LINUX
        exeFileName = "/linux-x64/mpc";
#else
        return;
#endif

                var input = $"{Application.dataPath}/../Assembly-CSharp.csproj";
                var output = $"{Application.dataPath}/Acderator/Scripts/Intense/Master/Master.Generated.cs";

                var psi = new ProcessStartInfo()
                {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        FileName = filePath + exeFileName,
                        Arguments = $@"-i ""{input}"" -o ""{output}""",
                        WorkingDirectory = rootPath
                };

                using var p = Process.Start(psi);
                var stdout = p.StandardOutput.ReadToEnd();
                var stderr = p.StandardError.ReadToEnd();
                p.WaitForExit();

                UnityEngine.Debug.Log($"exe: {filePath + exeFileName}");
                UnityEngine.Debug.Log($"input: {input}");
                UnityEngine.Debug.Log($"output: {output}");
                UnityEngine.Debug.Log($"ExitCode: {p.ExitCode}");

                if (!string.IsNullOrEmpty(stdout)) UnityEngine.Debug.Log(stdout);
                if (!string.IsNullOrEmpty(stderr)) UnityEngine.Debug.LogError(stderr);

                AssetDatabase.Refresh();
                UnityEngine.Debug.Log("end ExecuteMessagePackCodeGenerator");
        }
}
#endif