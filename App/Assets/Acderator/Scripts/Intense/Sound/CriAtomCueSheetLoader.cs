using CriWare;
using Cysharp.Threading.Tasks;
using Intense.Asset;
using UnityEngine;
using Zenject;

namespace Intense
{
    public sealed class CriAtomCueSheetLoader
    {
        [Inject] private readonly AssetBundleManager assetBundleManager;

        public async UniTask<CriAtomCueSheet> GetOrAddCueSheetAsync(string name, string sheetPath)
        {
            var sheet = CriAtom.GetCueSheet(name);
            var asset = await assetBundleManager.GetLoadedObjectAsync(sheetPath) as TextAsset;
            sheet ??= await AddCueSheetAsync(name, asset);
            if (sheet == default)
            {
                Debug.LogWarning(string.Format("{0} dose not exist", name));
                return default;
            }
            return sheet;
        }

        private async UniTask<CriAtomCueSheet> AddCueSheetAsync(string name, TextAsset asset)
        {
            var cueSheet = CriAtom.AddCueSheet(name, asset.bytes, "");
            await UniTask.WaitWhile(() => cueSheet.IsLoading);
            return cueSheet;
        }
    }
}