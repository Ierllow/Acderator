using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Intense.Data;
using Intense.UI;
using MessagePack;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Intense.Api
{
    internal class NetworkManager : SingletonMonoBehaviour<NetworkManager>
    {
        [SerializeField] private NetworkConfig networkConfigObject;

        public async UniTask<ResponseBase> RequestAsync(RequestBase request)
        {
            if (Application.internetReachability.EnumEquals(NetworkReachability.NotReachable))
            {
                return default;
            }

            Loading.Instance.ShowLoading();
            try
            {
                var requestBytes = MessagePackSerializer.Serialize(request.PostData);
                var session = LocalDataManager.Instance.System.Token;
                var url = !string.IsNullOrEmpty(session)
                    ? networkConfigObject.apiServerUrl + "/" + session + "/" + request.ApiKey
                    : networkConfigObject.apiServerUrl + "/" + request.ApiKey;

                using var www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
                www.SetRequestHeader("Content-Type", "application/x-msgpack");
                www.uploadHandler = new UploadHandlerRaw(requestBytes);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.timeout = 60;
                await www.SendWebRequest();
                if (!www.result.EnumEquals(UnityWebRequest.Result.Success)) return default;
                var responseBytes = www.downloadHandler.data;
                var responseData = MessagePackSerializer.Deserialize<Dictionary<string, object>>(responseBytes);
                var response = new ResponseBase(responseData);
                Debug.Log(ZString.Format("status: {0}, errorResponse: {1}", response.Status, response.ErrorMessage));
                return response;
            }
            finally
            {
                Loading.Instance.HideLoading();
            }
        }
    }
}