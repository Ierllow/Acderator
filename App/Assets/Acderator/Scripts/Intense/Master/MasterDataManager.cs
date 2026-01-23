using Cysharp.Threading.Tasks;
using Intense.Api;
using Intense.Asset;
using Intense.UI;
using Master;
using MessagePack.Resolvers;
using UnityEngine;
using UnityEngine.Networking;

namespace Intense.Master
{
    internal class MasterDataManager : SingletonMonoBehaviour<MasterDataManager>
    {
#if UNITY_EDITOR
        [SerializeField] private bool isMasterDebug = false;
#endif
        public bool IsInit { get; private set; } = false;

        internal MemoryDatabase MemoryDatabase { get; private set; }

        protected override void Awake()
        {
            if (IsInit) return;
            CompositeResolver.RegisterAndSetAsDefault(new[] { MasterMemoryResolver.Instance, GeneratedResolver.Instance, StandardResolver.Instance });
            base.Awake();
        }

        public async UniTask LoadMasterAsync()
        {
#if UNITY_EDITOR
            if (isMasterDebug)
            {
                MemoryDatabase = new MemoryDatabase(await RequestMasterData(await NetworkManager.Instance.GetUrl("master/master")));
                return;
            }
#endif
            MemoryDatabase = new MemoryDatabase((await AssetBundleManager.Instance.GetLoadedObjectAsync("master/master") as TextAsset).bytes);

            IsInit = true;
            await UniTask.WaitUntil(() => IsInit);
        }

#if UNITY_EDITOR
        public async UniTask<byte[]> RequestMasterData(string url)
        {
            Loading.Instance.ShowLoading();

            using var www = UnityWebRequest.Get(url);
            await www.SendWebRequest();
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError("www Error:" + www.error);
                Loading.Instance.HideLoading();
                return default;
            }
            Loading.Instance.HideLoading();
            return www.downloadHandler.data;
        }
#endif
    }
}