using Intense.Api;
using Song;
using UnityEditor;

public class _MenuItem
{
    private const string ITEM_NAME_PREFIX = "Tools/ScriptableObject/Create";

    [MenuItem(ITEM_NAME_PREFIX + "NetworkConfig")]
    private static void NetworkConfigToAsset() => ScriptableObjectUtils.ToAsset<NetworkConfig>("Intense/Api");
    [MenuItem(ITEM_NAME_PREFIX + "DebugConfig")]
    private static void DebugConfigToAsset() => ScriptableObjectUtils.ToAsset<DebugConfig>("Scene/Song/Data");
}