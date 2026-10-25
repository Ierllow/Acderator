using Intense.Api;
using Intense.Asset;
using Intense.UI;
using UnityEditor;

public static class EditorMenuItems
{
    private const string ScriptableObjectItemNamePrefix = "Tools/ScriptableObject/Create/";
    private const string CodeGenerateItemNamePrefix = "Tools/Code Generate/";

    [MenuItem(ScriptableObjectItemNamePrefix + "NetworkConfig")] private static void NetworkConfigToAsset() => ScriptableObjectUtils.ToAsset<NetworkConfig>("Intense/Api");
    [MenuItem(ScriptableObjectItemNamePrefix + "AddressableAssetConfig")] private static void AddressableAssetConfigToAsset() => ScriptableObjectUtils.ToAsset<AddressableAssetConfig>("Intense/Addressable");
    [MenuItem(ScriptableObjectItemNamePrefix + "ResultVisualConfig")] private static void ResultVisualConfigToAsset() => ScriptableObjectUtils.ToAsset<ResultVisualConfig>("Intense/UI");
    [MenuItem(CodeGenerateItemNamePrefix + "MasterMemory Only")] private static void GenerateMasterMemoryOnly() => CodeGenerator.GenerateMasterMemory();
    [MenuItem(CodeGenerateItemNamePrefix + "MessagePack Only")] private static void GenerateMessagePackOnly() => CodeGenerator.GenerateMessagePack();
    [MenuItem(CodeGenerateItemNamePrefix + "Generate Code All")] private static void GenerateAll() => CodeGenerator.GenerateAll();
    [MenuItem(CodeGenerateItemNamePrefix + "Generate Master from Spreadsheet")] private static void GenerateFromMenu() => SpreadsheetToMasterGenerator.GenerateFromMenu();
    [MenuItem("Tools/Screenshot")] private static void CaptureScreenshot() => Screenshot.ExecCaptureScreenshot();
}