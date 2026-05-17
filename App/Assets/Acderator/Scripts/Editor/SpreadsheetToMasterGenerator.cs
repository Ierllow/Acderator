using ClosedXML.Excel;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class SpreadsheetToMasterGenerator
{
    private enum KeyKind { None, Primary, Secondary }

    private sealed class MasterTable
    {
        public string ClassName { get; init; }
        public List<MasterField> FieldList { get; init; } = new();
    }

    private sealed class MasterField
    {
        public string SourceName { get; init; }
        public string FieldName { get; init; }
        public string CsType { get; init; }
        public KeyKind KeyKind { get; init; }
        public int SecondaryIndex { get; init; }
    }

    [Serializable]
    private class ServiceAccountKey
    {
        public string clientEmail;
        public string privateKey;
        public string tokenUri = "https://oauth2.googleapis.com/token";
    }

    [Serializable]
    private class TokenResponse
    {
        public string accessToken;
        public int expiresIn;
        public string tokenType;
    }

    private const string DefaultSpreadsheetPath = "Assets/Acderator/Master.xlsx";
    private const string OutputPath = "Assets/Acderator/Scripts/Intense/Master/Master.cs";
    private const string Namespace = "Intense.Master";

    private const string GoogleSheetId = "";
    private const AuthMode AuthenticationMode = AuthMode.PublicLink;

    private enum AuthMode { PublicLink, ServiceAccount, OAuth }

    private const string ServiceAccountKeyPath = "../MasterData/service-account.json";

    private const string DriveExportMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private static string cachedAccessToken;
    private static DateTime cachedAccessTokenExpiry = DateTime.MinValue;

    private const string DownloadCacheFileName = "AcderatorMaster.xlsx";
    private const int DownloadTimeoutSec = 30;

    private const float YellowHueMin = 40f, YellowHueMax = 65f;
    private const float OrangeHueMin = 10f, OrangeHueMax = 39f;
    private const float MinSaturation = 0.20f;
    private const float MinValue = 0.50f;

    public static void GenerateFromMenu() => UniTask.Void(async () =>
    {
        try
        {
            var path = await ResolveSpreadsheetPathAsync();
            if (!File.Exists(path))
            {
                Debug.LogError(string.Format("Spreadsheet not found: {0}", path));
                EditorUtility.DisplayDialog(
                    "Master spreadsheet not found",
                    string.Format("Place the master spreadsheet at:\n{0}\n\nor set GoogleSheetId in SpreadsheetMasterGenerator.cs.", path),
                    "OK");
                return;
            }

            Generate(path);
            AssetDatabase.Refresh();
            Debug.Log(string.Format("{0} regenerated from {1}", OutputPath, path));

        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("Generation failed: {0}", ex));
            EditorUtility.DisplayDialog("Master generation failed", ex.ToString(), "OK");
        }
    });

    private static void Generate(string spreadsheetPath)
    {
        using var workbook = new XLWorkbook(spreadsheetPath);
        var tables = workbook.Worksheets.Select(ReadSheet).Where(t => t.FieldList.Count > 0).ToList();
        if (tables.Count == 0)
        {
            throw new InvalidOperationException("No usable sheets found. Every sheet must have a header row with at least 1 field.");
        }

        var source = Emit(tables);
        var outputAbs = Path.GetFullPath(Path.Combine(Application.dataPath, "..", OutputPath));
        Directory.CreateDirectory(Path.GetDirectoryName(outputAbs));
        File.WriteAllText(outputAbs, source, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    public static void Generate() => UniTask.Void(async () =>
    {
        if (string.IsNullOrEmpty(GoogleSheetId))
        {
            var localPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", DefaultSpreadsheetPath));
            if (!File.Exists(localPath))
            {
                Debug.Log(string.Format("Spreadsheet not found at {0}; using existing hand-written Master.cs.", localPath));
                return;
            }
        }
        try
        {
            var path = await ResolveSpreadsheetPathAsync();
            Generate(path);
            Debug.Log(string.Format("{0} regenerated from {1}", OutputPath, path));
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("Generation failed: {0}", ex));
        }
    });

    private static MasterTable ReadSheet(IXLWorksheet sheet)
    {
        var table = new MasterTable { ClassName = sheet.Name.Trim() };
        var headerRow = sheet.Row(1);
        var typeRow = sheet.Row(2);

        var secondaryIndex = 0;
        for (var col = 1; ; col++)
        {
            var header = headerRow.Cell(col);
            var name = header.GetString().Trim();
            if (string.IsNullOrEmpty(name)) break;

            var typeText = typeRow.Cell(col).GetString().Trim().ToLowerInvariant();
            var csType = MapType(typeText);
            if (csType == null)
            {
                Debug.LogWarning(string.Format("Sheet '{0}' column '{1}': unsupported type '{2}'. Skipping field.", sheet.Name, name, typeText));
                continue;
            }

            var keyKind = ClassifyHeaderColor(header.Style.Fill.BackgroundColor);
            var fieldName = ToPascalCase(name);

            table.FieldList.Add(new MasterField
            {
                SourceName = name,
                FieldName = fieldName,
                CsType = csType,
                KeyKind = keyKind,
                SecondaryIndex = keyKind == KeyKind.Secondary ? secondaryIndex++ : -1,
            });
        }

        return table;
    }

    private static KeyKind ClassifyHeaderColor(XLColor xlColor)
    {
        if (xlColor?.HasValue == false) return KeyKind.None;
        var color = xlColor.Color;
        if (color.R >= 250 && color.G >= 250 && color.B >= 250) return KeyKind.None;

        ColorToHsv(color.R / 255f, color.G / 255f, color.B / 255f, out var h, out var s, out var v);
        return h switch
        {
            _ when s < MinSaturation || v < MinValue => KeyKind.None,
            >= YellowHueMin and <= YellowHueMax => KeyKind.Primary,
            >= OrangeHueMin and <= OrangeHueMax => KeyKind.Secondary,
            _ => KeyKind.None,
        };
    }

    private static void ColorToHsv(float r, float g, float b, out float h, out float s, out float v)
    {
        var max = Mathf.Max(r, Mathf.Max(g, b));
        var min = Mathf.Min(r, Mathf.Min(g, b));
        v = max;
        var delta = max - min;
        s = max <= 0f ? 0f : delta / max;
        if (delta <= 0f)
        {
            h = 0f;
            return;
        }
        var hue = max == r ? (g - b) / delta % 6f : max == g ? (b - r) / delta + 2f : (r - g) / delta + 4f;
        hue *= 60f;
        if (hue < 0f) hue += 360f;
        h = hue;
    }

    private static string MapType(string text) => text switch
    {
        "" => null,
        "int" => "int",
        "int32" => "int",
        "long" => "long",
        "int64" => "long",
        "float" => "float",
        "single" => "float",
        "double" => "double",
        "bool" => "bool",
        "boolean" => "bool",
        "string" => "string",
        "text" => "string",
        _ => null,
    };

    private static string DefaultValueLiteral(string csType) => csType switch
    {
        "string" => "string.Empty",
        "bool" => "false",
        "int" => "0",
        "long" => "0L",
        "float" => "0f",
        "double" => "0d",
        _ => "default",
    };

    private static string ParseExpression(string csType, string varName) => csType switch
    {
        "string" => string.Format("{0}.ToString()", varName),
        "bool" => string.Format("bool.Parse({0}.ToString())", varName),
        "int" => string.Format("int.Parse({0}.ToString())", varName),
        "long" => string.Format("long.Parse({0}.ToString())", varName),
        "float" => string.Format("float.Parse({0}.ToString())", varName),
        "double" => string.Format("double.Parse({0}.ToString())", varName),
        _ => string.Format("({0}){1}", csType, varName),
    };

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var sb = new StringBuilder(input.Length);
        var upperNext = true;
        foreach (var ch in input)
        {
            if (ch == '_' || ch == '-' || ch == ' ')
            {
                upperNext = true;
                continue;
            }
            sb.Append(upperNext ? char.ToUpperInvariant(ch) : ch);
            upperNext = false;
        }
        return sb.ToString();
    }

    private static string ToCamelCase(string pascal) => string.IsNullOrEmpty(pascal) ? pascal : char.ToLowerInvariant(pascal[0]) + pascal[1..];

    private static string Emit(List<MasterTable> tables)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated>");
        sb.AppendLine("//   Generated by SpreadsheetToMasterGenerator. Do not edit by hand.");
        sb.AppendLine("//   Re-run: Tools > Code Generate > Generate Master from Spreadsheet");
        sb.AppendLine("// </auto-generated>");
        sb.AppendLine("using MasterMemory;");
        sb.AppendLine("using MessagePack;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();
        sb.Append("namespace ");
        sb.AppendLine(Namespace);
        sb.AppendLine("{");

        sb.AppendLine("    [MessagePackObject(true)]");
        for (var i = 0; i < tables.Count; i++)
        {
            sb.Append("    [Union(");
            sb.Append(i);
            sb.Append(", typeof(");
            sb.Append(tables[i].ClassName);
            sb.AppendLine("))]");
        }
        sb.AppendLine("    public abstract class BaseMaster { }");

        foreach (var t in tables)
        {
            sb.AppendLine();
            EmitTable(sb, t);
        }

        sb.Append("}");
        var text = sb.ToString();
        return text.TrimEnd('\r', '\n');
    }

    private static void EmitTable(StringBuilder sb, MasterTable t)
    {
        sb.Append("    [MessagePackObject(true), MemoryTable(\"").Append(t.ClassName).AppendLine("\")]");
        sb.Append("    public class ");
        sb.Append(t.ClassName);
        sb.AppendLine(" : BaseMaster");
        sb.AppendLine("    {");

        foreach (var f in t.FieldList)
        {
            var annotation = f.KeyKind switch
            {
                KeyKind.Primary => "[PrimaryKey] ",
                KeyKind.Secondary => string.Format("[SecondaryKey({0})] ", f.SecondaryIndex),
                _ => string.Empty,
            };
            sb.Append("        ");
            sb.Append(annotation);
            sb.Append("public ");
            sb.Append(f.CsType);
            sb.Append(' ');
            sb.Append(f.FieldName);
            sb.AppendLine(" { get; set; }");
        }

        sb.AppendLine();
        sb.Append("        public static ");
        sb.Append(t.ClassName);
        sb.AppendLine(" From(Dictionary<string, object> masterDict) => new()");
        sb.AppendLine("        {");
        foreach (var f in t.FieldList)
        {
            var localVar = ToCamelCase(f.FieldName);
            var defaultLit = DefaultValueLiteral(f.CsType);
            var parseExpr = ParseExpression(f.CsType, localVar);
            var condition = f.CsType == "bool"
                ? string.Format("masterDict.TryGetValue(\"{0}\", out var {1}) && {2}", f.SourceName, localVar, parseExpr)
                : string.Format("masterDict.TryGetValue(\"{0}\", out var {1}) ? {2} : {3}", f.SourceName, localVar, parseExpr, defaultLit);
            sb.Append("            ");
            sb.Append(f.FieldName);
            sb.Append(" = ");
            sb.Append(condition);
            sb.AppendLine(",");
        }
        sb.AppendLine("        };");
        sb.AppendLine("    }");
    }

    private static async UniTask<string> ResolveSpreadsheetPathAsync()
    {
        if (string.IsNullOrEmpty(GoogleSheetId))
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", DefaultSpreadsheetPath));
        }

        var cachePath = Path.Combine(Path.GetTempPath(), DownloadCacheFileName);
        await DownloadSpreadsheet(GoogleSheetId, cachePath);
        return cachePath;
    }

    private static async UniTask DownloadSpreadsheet(string sheetId, string cachePath)
    {
        using var client = new System.Net.Http.HttpClient();
        client.Timeout = TimeSpan.FromSeconds(DownloadTimeoutSec);

        var url = BuildDownloadUrl(AuthenticationMode, sheetId);
        var authToken = AuthenticationMode switch
        {
            AuthMode.PublicLink => null,
            AuthMode.ServiceAccount => await GetServiceAccountAccessTokenAsync(),
            AuthMode.OAuth => GetOAuthAccessToken(),
            _ => throw new InvalidOperationException(string.Format("Unsupported AuthMode: {0}", AuthenticationMode)),
        };
        if (!string.IsNullOrEmpty(authToken))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
        }

        Debug.Log(string.Format("Downloading master spreadsheet via {0} from {1}", AuthenticationMode, url));
        var response = await client.GetAsync(url).AsUniTask();
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync().AsUniTask();
            throw new InvalidOperationException(string.Format("Download failed ({0}): {1}", response.StatusCode, body));
        }
        var bytes = await response.Content.ReadAsByteArrayAsync().AsUniTask();
        File.WriteAllBytes(cachePath, bytes);
        Debug.Log(string.Format("Saved spreadsheet cache to {0} ({1} bytes)", cachePath, bytes.Length));
    }

    private static string BuildDownloadUrl(AuthMode mode, string sheetId) => mode switch
    {
        AuthMode.PublicLink => string.Format("https://docs.google.com/spreadsheets/d/{0}/export?format=xlsx", sheetId),
        _ => string.Format("https://www.googleapis.com/drive/v3/files/{0}/export?mimeType={1}", sheetId, Uri.EscapeDataString(DriveExportMimeType)),
    };

    private static async UniTask<string> GetServiceAccountAccessTokenAsync()
    {
        if (cachedAccessToken != null && cachedAccessTokenExpiry > DateTime.UtcNow.AddMinutes(1))
        {
            return cachedAccessToken;
        }

        var keyPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ServiceAccountKeyPath));
        if (!File.Exists(keyPath))
        {
            throw new FileNotFoundException(string.Format("Service account key file not found at {0}. Place the JSON key from Google Cloud Console there, and ensure it's gitignored.", keyPath));
        }

        var key = JsonUtility.FromJson<ServiceAccountKey>(File.ReadAllText(keyPath));
        if (string.IsNullOrEmpty(key.clientEmail) || string.IsNullOrEmpty(key.privateKey))
        {
            throw new InvalidOperationException("Service account key is missing client_email or private_key.");
        }

        var jwt = BuildServiceAccountJwt(key);
        var tokenResponse = await ExchangeJwtForTokenAsync(jwt, key.tokenUri);
        cachedAccessToken = tokenResponse.accessToken;
        cachedAccessTokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.expiresIn);
        Debug.Log(string.Format("Service account access token acquired (expires in {0}s)", tokenResponse.expiresIn));
        return cachedAccessToken;
    }

    private static string BuildServiceAccountJwt(ServiceAccountKey key)
    {
        var headerJson = "{\"alg\":\"RS256\",\"typ\":\"JWT\"}";
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var claimsJson = string.Format(
            "{{\"iss\":\"{0}\",\"scope\":\"https://www.googleapis.com/auth/drive.readonly\",\"aud\":\"{1}\",\"iat\":{2},\"exp\":{3}}}",
            key.clientEmail, key.tokenUri, now, now + 3600);

        var headerB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
        var claimsB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(claimsJson));
        var unsigned = string.Format("{0}.{1}", headerB64, claimsB64);

        using var rsa = CreateRsaFromPkcs8Pem(key.privateKey);
        var signature = rsa.SignData(
            Encoding.UTF8.GetBytes(unsigned),
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            System.Security.Cryptography.RSASignaturePadding.Pkcs1);

        return string.Format("{0}.{1}", unsigned, Base64UrlEncode(signature));
    }

    private static System.Security.Cryptography.RSA CreateRsaFromPkcs8Pem(string pem)
    {
        const string header = "-----BEGIN PRIVATE KEY-----";
        const string footer = "-----END PRIVATE KEY-----";
        var headerIndex = pem.IndexOf(header, StringComparison.Ordinal);
        var footerIndex = pem.IndexOf(footer, StringComparison.Ordinal);
        if (headerIndex < 0 || footerIndex < 0 || footerIndex <= headerIndex)
        {
            throw new InvalidOperationException("Service account private_key must be PKCS#8 PEM ...");
        }
        var bodyStart = headerIndex + header.Length;
        pem[bodyStart..footerIndex].Replace("\r", string.Empty);
        pem[bodyStart..footerIndex].Replace("\n", string.Empty);
        var base64 = pem[bodyStart..footerIndex].Replace(" ", string.Empty);
        var der = Convert.FromBase64String(base64);
        var rsa = System.Security.Cryptography.RSA.Create();
        rsa.ImportPkcs8PrivateKey(der, out _);
        return rsa;
    }

    private static async UniTask<TokenResponse> ExchangeJwtForTokenAsync(string jwt, string tokenUri)
    {
        using var client = new System.Net.Http.HttpClient();
        var content = new System.Net.Http.FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
            new KeyValuePair<string, string>("assertion", jwt),
        });
        var response = await client.PostAsync(tokenUri, content).AsUniTask();
        var body = await response.Content.ReadAsStringAsync().AsUniTask();
        return !response.IsSuccessStatusCode
            ? throw new InvalidOperationException(string.Format("Token endpoint returned {0}: {1}", response.StatusCode, body))
            : JsonUtility.FromJson<TokenResponse>(body);
    }

    private static string Base64UrlEncode(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string GetOAuthAccessToken() => throw new NotImplementedException("OAuth user flow is not implemented. See comment on GetOAuthAccessToken for design notes.");
}