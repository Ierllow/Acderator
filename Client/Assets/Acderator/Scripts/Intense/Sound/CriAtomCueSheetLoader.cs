using CriWare;
using Cysharp.Threading.Tasks;
using Intense.Asset;
using System;
using UnityEngine;
using Zenject;

namespace Intense
{
    public sealed class CriAtomCueSheetLoader
    {
        [Inject] private readonly AddressableAssetManager addressableAssetManager;

        public UniTask<CriAtomCueSheet> GetOrAddCueSheetAsync(string name, TextAssetReference reference)
        {
            var sheet = CriAtom.GetCueSheet(name);
            return sheet != default ? UniTask.FromResult(sheet) : GetOrAddCueSheetAsync(name, () => addressableAssetManager.LoadAssetAsync(reference));
        }

        public UniTask<CriAtomCueSheet> GetOrAddCueSheetAsync(string name, AddressableAssetAddress address)
        {
            var sheet = CriAtom.GetCueSheet(name);
            return sheet != default ? UniTask.FromResult(sheet) : GetOrAddCueSheetAsync(name, () => addressableAssetManager.LoadAssetAsync<TextAsset>(address));
        }

        private async UniTask<CriAtomCueSheet> GetOrAddCueSheetAsync(string name, Func<UniTask<TextAsset>> loadAsset)
        {
            var asset = await loadAsset();
            if (asset == default)
            {
                Debug.LogWarning(string.Format("{0} dose not exist", name));
                return default;
            }
            return await AddCueSheetAsync(name, asset);
        }

        private async UniTask<CriAtomCueSheet> AddCueSheetAsync(string name, TextAsset asset)
        {
            var cueSheet = CriAtom.AddCueSheet(name, asset.bytes, "");
            await UniTask.WaitWhile(() => cueSheet.IsLoading);
            return cueSheet;
        }
    }
}