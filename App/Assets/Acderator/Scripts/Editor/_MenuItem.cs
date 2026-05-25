using Intense.Api;
using Song;
using UnityEditor;

public class _MenuItem
{
    private const string SCRIPTABLE_OBJECT_ITEM_NAME_PREFIX = "Tools/ScriptableObject/Create/";
    private const string CODE_GENERATE_ITEM_NAME_PREFIX = "Tools/Code Generate/";

    [MenuItem(SCRIPTABLE_OBJECT_ITEM_NAME_PREFIX + "NetworkConfig")] private static void NetworkConfigToAsset() => ScriptableObjectUtils.ToAsset<NetworkConfig>("Intense/Api");
    [MenuItem(CODE_GENERATE_ITEM_NAME_PREFIX + "MasterMemory Only")] private static void GenerateMasterMemoryOnly() => CodeGenerators.ExecuteMasterMemoryCodeGenerator();
    [MenuItem(CODE_GENERATE_ITEM_NAME_PREFIX + "MessagePack Only")] private static void GenerateMessagePackOnly() => CodeGenerators.ExecuteMessagePackCodeGenerator();
    [MenuItem(CODE_GENERATE_ITEM_NAME_PREFIX + "Generate Code All")] private static void GenerateAll() => CodeGenerators.GenerateAll();
    [MenuItem(CODE_GENERATE_ITEM_NAME_PREFIX + "Generate Master from Spreadsheet")] private static void GenerateFromMenu() => SpreadsheetToMasterGenerator.GenerateFromMenu();
    [MenuItem("Tools/Screenshot")] private static void CaptureScreenshot() => Screenshot.ExecCaptureScreenshot();
}