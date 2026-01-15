using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Storage;
using Intense.Data;
using Intense.UI;
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine;

namespace Intense.Api
{
    internal class NetworkManager : SingletonMonoBehaviour<NetworkManager>
    {
        public bool IsLoggedIn { get; private set; }
        public bool IsInit { get; private set; }

        protected override void Awake()
        {
            UniTask.Void(async () =>
            {
                await FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().AsUniTask();
                IsInit = true;
            });
            base.Awake();
        }

        public async UniTask<string> GetUrl(string path)
        {
            if (!IsInit) await UniTask.WaitUntil(() => IsInit);

            var storageUrl = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(""); //
            var storagePath = storageUrl.Child(path);
            var downloadUrl = await storagePath.GetDownloadUrlAsync().AsUniTask();
            return downloadUrl.AbsoluteUri;
        }

        public async UniTask<ResponseBase<LoginResult>> LogInAnonymouslyAsync()
        {
            if (IsLoggedIn) return null;

            Loading.Instance.ShowLoading();

            var userId = LocalDataManager.Instance.LocalUser.UserId;
            var passWard = LocalDataManager.Instance.LocalUser.PassWard;
            var customId = "";
            var request = new LoginWithCustomIDRequest
            {
                CustomId = userId,
                CreateAccount = false,
            };
            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(passWard))
            {
                request.CustomId = customId = Guid.NewGuid().ToString();
                request.CreateAccount = true;
            }

            LoginResult result = null;
            PlayFabError error = null;

            PlayFabClientAPI.LoginWithCustomID(request, x => result = x, x => error = x);
            await UniTask.WaitUntil(() => result != null || error != null);

            if (result != null)
            {
                if (result.NewlyCreated)
                {
                    LocalDataManager.Instance.LocalUser.UserId = request.CustomId;
                    LocalDataManager.Instance.LocalUser.PassWard = request.CustomId;
                }
                IsLoggedIn = true;
            }
            Debug.Log(ZString.Format("errorResponse: {0}", error?.GenerateErrorReport()));
            Loading.Instance.HideLoading();

            return new(result, error);
        }

        public async UniTask<ResponseBase<UpdateUserDataResult>> RequestAsync(RequestBase request)
        {
            Loading.Instance.ShowLoading();

            UpdateUserDataResult result = null;
            PlayFabError error = null;

            PlayFabClientAPI.UpdateUserData(request.CreateUpdateUserDataRequest(), x => result = x, x => error = x);
            await UniTask.WaitUntil(() => result != null || error != null);
            Debug.Log(ZString.Format("errorResponse: {0}", error?.GenerateErrorReport()));
            Loading.Instance.HideLoading();

            return new(result, error);
        }
    }
}