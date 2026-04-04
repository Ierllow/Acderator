using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Intense.UI;
using MessagePack;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace Intense.Api
{
    internal class NetworkManager : MonoBehaviour
    {
        [SerializeField] private NetworkConfig networkConfigObject;

        [Inject] private Loading loading;

        public async UniTask<ResponseBase> RequestAsync(RequestBase request)
        {
            if (Application.internetReachability.EnumEquals(NetworkReachability.NotReachable))
            {
                return default;
            }

            loading.ShowLoading();
            try
            {
                var requestBytes = MessagePackSerializer.Serialize(request.PostData);
                var session = PlayerPrefsValues.TK;
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
                loading.HideLoading();
            }
        }
    }
}
