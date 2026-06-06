using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Intense.Asset
{
    public sealed class AddressableAssetConfig : ScriptableObject
    {
        [SerializeField] private AssetLabelReference[] songSelectLabels;
        [SerializeField] private AssetLabelReference[] songLabels;
        [SerializeField] private AssetLabelReference[] resultLabels;

        [SerializeField] private TextAssetReference songSe;
        [SerializeField] private TextAssetReference bgm;

        [SerializeField] private string assetServerUrl = "";
        [SerializeField] private int loadCoolDownMilliseconds = 500;

        internal TextAssetReference SongSe => songSe;
        internal TextAssetReference Bgm => bgm;
        internal string AssetServerUrl => assetServerUrl;
        internal int LoadCoolDownMilliseconds => loadCoolDownMilliseconds;

        internal AssetLabelReference[] GetLabels(ESceneType sceneType) => sceneType switch
        {
            ESceneType.SongSelect => songSelectLabels,
            ESceneType.Song => songLabels,
            ESceneType.Result => resultLabels,
            _ => new AssetLabelReference[] { },
        };
    }
}