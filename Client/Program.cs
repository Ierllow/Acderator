using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;

internal static class Program
{
    private const string SourceRoot = "Assets/Acderator";
    private const string IgnoredMasterRoot = "Assets/Acderator/Scripts/Intense/Master/";

    public static async Task<int> Main(string[] files)
    {
        var fix = files.Contains("--fix", StringComparer.Ordinal);
        files = files.Where(x => x != "--fix").ToArray();
        if (files.Length == 0) files = GetDefaultTargetFiles().ToArray();

        var targetFiles = files.Select(NormalizePath).Where(x => File.Exists(x) && !IsIgnored(x)).ToHashSet(StringComparer.Ordinal);
        if (targetFiles.Count == 0)
        {
            Console.WriteLine("No C# files to check.");
            return 0;
        }

        var compilation = CreateCompilation();
        var diagnostics = (await GetDiagnostics(compilation))
            .Where(x => targetFiles.Contains(NormalizePath(x.Location.SourceTree!.FilePath)))
            .OrderBy(x => NormalizePath(x.Location.SourceTree!.FilePath))
            .ThenBy(x => x.Location.GetLineSpan().StartLinePosition.Line)
            .ToArray();
        var usingOrderViolations = targetFiles.SelectMany(CheckUsingOrder).OrderBy(x => x.Path).ThenBy(x => x.Line).ToArray();
        var finalNewlineViolations = targetFiles.Where(HasFinalNewline).Order(StringComparer.Ordinal).ToArray();

        foreach (var diagnostic in diagnostics)
        {
            var lineSpan = diagnostic.Location.GetLineSpan();
            Console.Error.WriteLine("{0}({1},{2}): {3} {4}", NormalizePath(lineSpan.Path), lineSpan.StartLinePosition.Line + 1, lineSpan.StartLinePosition.Character + 1, diagnostic.Id, diagnostic.GetMessage());
        }

        foreach (var violation in usingOrderViolations)
        {
            Console.Error.WriteLine("{0}({1},1): ACD0001 using directives must be sorted alphabetically.", violation.Path, violation.Line);
        }

        foreach (var path in finalNewlineViolations)
        {
            Console.Error.WriteLine("{0}: ACD0002 C# files must not end with a newline.", path);
        }

        if (fix)
        {
            RemoveUnusedUsingLines(diagnostics.Where(x => x.Id == "CS8019"));
            SortUsingBlocks(targetFiles);
            TrimFinalNewlines(targetFiles);
            return 0;
        }

        if (diagnostics.Length > 0 || usingOrderViolations.Length > 0 || finalNewlineViolations.Length > 0) return 1;
        Console.WriteLine("No coding rule violations found.");
        return 0;
    }

    private static IEnumerable<string> GetDefaultTargetFiles()
        => Directory.EnumerateFiles(SourceRoot, "*.cs", SearchOption.AllDirectories).Where(x => !IsIgnored(x)).Append("Program.cs");

    private static IEnumerable<UsingOrderViolation> CheckUsingOrder(string path)
    {
        var lines = File.ReadAllText(path).Replace("\r\n", "\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            if (!IsUsingLine(lines[i])) continue;

            var start = i;
            while (i + 1 < lines.Length && IsUsingLine(lines[i + 1])) i++;

            var block = lines[start..(i + 1)];
            var sorted = block.Order(UsingDirectiveComparer.Instance).ToArray();
            if (!block.SequenceEqual(sorted)) yield return new(path, start + 1);
        }
    }

    private static void SortUsingBlocks(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            var text = File.ReadAllText(path);
            var newLine = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
            var lines = text.Replace("\r\n", "\n").Split('\n').ToList();
            for (var i = 0; i < lines.Count; i++)
            {
                if (!IsUsingLine(lines[i])) continue;

                var start = i;
                while (i + 1 < lines.Count && IsUsingLine(lines[i + 1])) i++;

                var sorted = lines.GetRange(start, i - start + 1).Order(UsingDirectiveComparer.Instance).ToArray();
                for (var j = 0; j < sorted.Length; j++) lines[start + j] = sorted[j];
            }

            File.WriteAllText(path, string.Join(newLine, lines).TrimEnd('\r', '\n'), new UTF8Encoding(false));
        }
    }

    private static bool IsUsingLine(string line) => line.TrimStart('\uFEFF').StartsWith("using ", StringComparison.Ordinal);

    private static bool HasFinalNewline(string path)
    {
        var info = new FileInfo(path);
        if (!info.Exists || info.Length == 0) return false;

        var bytes = File.ReadAllBytes(path);
        var last = bytes[^1];
        return last is (byte)'\n' or (byte)'\r';
    }

    private static void TrimFinalNewlines(IEnumerable<string> paths)
    {
        foreach (var path in paths.Where(HasFinalNewline))
        {
            File.WriteAllText(path, File.ReadAllText(path).TrimEnd('\r', '\n'), new UTF8Encoding(false));
        }
    }

    private static async Task<IEnumerable<Diagnostic>> GetDiagnostics(CSharpCompilation compilation)
    {
        var compilerDiagnostics = compilation.GetDiagnostics();
        var compilationDiagnostics = compilerDiagnostics.Any(x => x.Severity == DiagnosticSeverity.Error && x.Id != "CS8019")
            ? []
            : compilerDiagnostics.Where(x => x.Id == "CS8019" && x.Location.SourceTree != null);
        var analyzers = GetAnalyzerReferences().SelectMany(x => x.GetAnalyzers(LanguageNames.CSharp)).ToImmutableArray();
        if (analyzers.Length == 0) return compilationDiagnostics;

        var options = new AnalyzerOptions([], new EditorConfigOptionsProvider(ReadEditorConfigOptions()));
        var compilationWithAnalyzersOptions = new CompilationWithAnalyzersOptions(options, null, true, false, true);
        var analyzerDiagnostics = await compilation.WithAnalyzers(analyzers, compilationWithAnalyzersOptions).GetAnalyzerDiagnosticsAsync();
        return compilationDiagnostics.Concat(analyzerDiagnostics.Where(IsStyleDiagnostic));
    }

    private static bool IsStyleDiagnostic(Diagnostic diagnostic)
        => !diagnostic.IsSuppressed
            && diagnostic.Location.SourceTree != null
            && diagnostic.Id.StartsWith("IDE", StringComparison.Ordinal)
            && diagnostic.Severity is DiagnosticSeverity.Warning or DiagnosticSeverity.Error;

    private static void RemoveUnusedUsingLines(IEnumerable<Diagnostic> diagnostics)
    {
        foreach (var group in diagnostics.GroupBy(x => NormalizePath(x.Location.GetLineSpan().Path)))
        {
            var text = File.ReadAllText(group.Key);
            var newLine = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
            var lines = text.Replace("\r\n", "\n").Split('\n').ToList();
            foreach (var line in group.Select(x => x.Location.GetLineSpan().StartLinePosition.Line).Distinct().OrderDescending())
            {
                if (line < 0 || line >= lines.Count || !lines[line].TrimStart().StartsWith("using ", StringComparison.Ordinal)) continue;
                lines.RemoveAt(line);
            }

            File.WriteAllText(group.Key, string.Join(newLine, lines).TrimEnd('\r', '\n'), new UTF8Encoding(false));
        }
    }

    private static CSharpCompilation CreateCompilation()
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest).WithPreprocessorSymbols("UNITY_EDITOR");
        var syntaxTrees = Directory.EnumerateFiles(SourceRoot, "*.cs", SearchOption.AllDirectories)
            .Select(NormalizePath)
            .Select(x => CSharpSyntaxTree.ParseText(File.ReadAllText(x), parseOptions, x));
        return CSharpCompilation.Create("UnusedUsingCheck", syntaxTrees, CreateMetadataReferences(), new(OutputKind.DynamicallyLinkedLibrary));
    }

    private static IEnumerable<MetadataReference> CreateMetadataReferences()
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in GetTrustedPlatformAssemblies()) paths.Add(path);
        foreach (var directory in GetUnityManagedDirectories())
        {
            foreach (var path in GetDllReferences(directory)) paths.Add(path);
        }

        foreach (var path in GetDllReferences("Library/PackageCache")) paths.Add(path);
        foreach (var path in GetDllReferences("Assets")) paths.Add(path);
        foreach (var path in GetDllReferences("Temp/bin/Debug")) paths.Add(path);

        foreach (var path in paths)
        {
            MetadataReference reference;
            try
            {
                reference = MetadataReference.CreateFromFile(path);
            }
            catch
            {
                continue;
            }

            yield return reference;
        }
    }

    private static IEnumerable<string> GetTrustedPlatformAssemblies()
    {
        var assemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        return string.IsNullOrEmpty(assemblies) ? [] : assemblies.Split(Path.PathSeparator);
    }

    private static IEnumerable<string> GetDllReferences(string directory)
        => Directory.Exists(directory) ? Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories) : [];

    private static IEnumerable<string> GetUnityManagedDirectories()
    {
        foreach (var path in GetUnityEditorRoots())
        {
            yield return Path.Combine(path, "Data", "Managed");
            yield return Path.Combine(path, "Data", "Managed", "UnityEngine");
        }
    }

    private static IEnumerable<string> GetUnityEditorRoots()
    {
        foreach (var variable in new[] { "UNITY_EDITOR_PATH", "UNITY_PATH" })
        {
            var path = Environment.GetEnvironmentVariable(variable);
            if (!string.IsNullOrEmpty(path)) yield return Directory.Exists(path) ? path : Path.GetDirectoryName(path)!;
        }

        var version = GetUnityVersion();
        if (!string.IsNullOrEmpty(version))
        {
            yield return Path.Combine("C:/Program Files/Unity/Hub/Editor", version, "Editor");
            yield return Path.Combine("/opt/unity/Editor");
            yield return Path.Combine("/Applications/Unity/Hub/Editor", version, "Unity.app", "Contents");
        }
    }

    private static string GetUnityVersion()
    {
        const string versionPrefix = "m_EditorVersion:";
        var path = "ProjectSettings/ProjectVersion.txt";
        if (!File.Exists(path)) return string.Empty;
        return File.ReadLines(path).FirstOrDefault(x => x.StartsWith(versionPrefix, StringComparison.Ordinal))?[versionPrefix.Length..].Trim() ?? string.Empty;
    }

    private static IEnumerable<AnalyzerFileReference> GetAnalyzerReferences()
    {
        var packageRoot = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
        if (string.IsNullOrEmpty(packageRoot)) packageRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        var analyzerRoot = Path.Combine(packageRoot, "microsoft.codeanalysis.csharp.codestyle", "4.12.0", "analyzers", "dotnet", "cs");
        if (!Directory.Exists(analyzerRoot)) yield break;

        var loader = new AnalyzerAssemblyLoader();
        foreach (var path in Directory.EnumerateFiles(analyzerRoot, "Microsoft.CodeAnalysis*.dll"))
        {
            loader.AddDependencyLocation(path);
            yield return new(path, loader);
        }
    }

    private static IReadOnlyDictionary<string, string> ReadEditorConfigOptions()
    {
        var path = Path.Combine(SourceRoot, ".editorconfig");
        if (!File.Exists(path)) return new Dictionary<string, string>();

        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var inCSharpSection = false;
        foreach (var line in File.ReadLines(path).Select(x => x.Trim()))
        {
            if (line.Length == 0 || line.StartsWith('#')) continue;
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                inCSharpSection = line == "[*.cs]";
                continue;
            }

            if (!inCSharpSection) continue;
            var separatorIndex = line.IndexOf('=');
            if (separatorIndex < 0) continue;
            options[line[..separatorIndex].Trim()] = line[(separatorIndex + 1)..].Trim();
        }

        return options;
    }

    private static bool IsIgnored(string path) => NormalizePath(path).StartsWith(NormalizePath(IgnoredMasterRoot), StringComparison.Ordinal);

    private static string NormalizePath(string path) => Path.GetFullPath(path).Replace('\\', '/');
}

internal sealed class AnalyzerAssemblyLoader : IAnalyzerAssemblyLoader
{
    private readonly HashSet<string> dependencyLocations = new(StringComparer.OrdinalIgnoreCase);

    public void AddDependencyLocation(string fullPath) => dependencyLocations.Add(fullPath);

    public Assembly LoadFromPath(string fullPath)
    {
        foreach (var dependencyLocation in dependencyLocations) Assembly.LoadFrom(dependencyLocation);
        return Assembly.LoadFrom(fullPath);
    }
}

internal sealed class EditorConfigOptionsProvider(IReadOnlyDictionary<string, string> options) : AnalyzerConfigOptionsProvider
{
    private readonly AnalyzerConfigOptions analyzerConfigOptions = new EditorConfigOptions(options);

    public override AnalyzerConfigOptions GlobalOptions => analyzerConfigOptions;

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => analyzerConfigOptions;

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => analyzerConfigOptions;
}

internal sealed class EditorConfigOptions(IReadOnlyDictionary<string, string> options) : AnalyzerConfigOptions
{
    public override bool TryGetValue(string key, out string value) => options.TryGetValue(key, out value!);
}

internal readonly record struct UsingOrderViolation(string Path, int Line);

internal sealed class UsingDirectiveComparer : IComparer<string>
{
    public static readonly UsingDirectiveComparer Instance = new();

    public int Compare(string? x, string? y)
    {
        var xKey = GetKey(x);
        var yKey = GetKey(y);
        var namespaceComparison = string.Compare(xKey.Namespace, yKey.Namespace, StringComparison.OrdinalIgnoreCase);
        return namespaceComparison != 0 ? namespaceComparison : xKey.IsStatic.CompareTo(yKey.IsStatic);
    }

    private static (string Namespace, bool IsStatic) GetKey(string? line)
    {
        var value = (line ?? string.Empty).Trim().TrimStart('\uFEFF');
        if (!value.StartsWith("using ", StringComparison.Ordinal)) return (value, false);

        value = value["using ".Length..].Trim();
        var isStatic = value.StartsWith("static ", StringComparison.Ordinal);
        if (isStatic) value = value["static ".Length..].Trim();
        return (value.TrimEnd(';').Trim(), isStatic);
    }
}