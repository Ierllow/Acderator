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
    public enum NetworkError
    {
        None = 0,
        BadRequest,
        Unauthorized,
        Forbidden,
        NotFound,
        Conflict,
        PreconditionFailed,
        Maintenance,
        UnsupportedMediaType,
        TooManyRequests,
        PayloadTooLarge,
        InvalidRequestFormat,
        ResponseFailure,
        DataFailure,
        ResourceFailure,
        ServiceFailure,
    }

    internal class NetworkManager : MonoBehaviour
    {
        [SerializeField] private NetworkConfig networkConfigObject;

        [Inject] private Loading loading;

        public async UniTask<ResponseBase> RequestAsync(RequestBase request)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return default;
            }

            loading.ShowLoading();
            try
            {
                var session = PlayerPrefsValues.TK;
                var url = string.Format("{0}/{1}", networkConfigObject.apiServerUrl, request.ApiKey);

                using var www = new UnityWebRequest(url, request.HttpMethod);
                www.SetApiRequestHeaders(session, PlayerPrefsValues.MV);
                if (request.HttpMethod != UnityWebRequest.kHttpVerbGET) www.uploadHandler = new UploadHandlerRaw(MessagePackSerializer.Serialize(request.PostData));
                www.downloadHandler = new DownloadHandlerBuffer();
                www.timeout = 60;
                await www.SendWebRequest();
                var responseBytes = www.downloadHandler.data;
                if (www.result != UnityWebRequest.Result.Success) return default;
                var responseData = MessagePackSerializer.Deserialize<Dictionary<string, object>>(responseBytes);
                var response = request.CreateResponse(responseData);
                Debug.Log(string.Format("errorCode: {0}, networkError: {1}, errorResponse: {2}", response.ErrorCode, response.NetworkError, response.ErrorMessage));
                return response;
            }
            finally
            {
                loading.HideLoading();
            }
        }
    }
}