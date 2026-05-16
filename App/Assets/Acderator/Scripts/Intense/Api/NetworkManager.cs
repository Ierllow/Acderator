using Cysharp.Threading.Tasks;
using Intense.UI;
using MessagePack;
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
        [Inject] private IApiSession apiSession;

        public async UniTask<ResponseBase> RequestAsync(RequestBase request)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return default;
            }

            loading.ShowLoading();
            try
            {
                var sessionToken = apiSession.Token;
                var url = string.Format("{0}/{1}", networkConfigObject.apiServerUrl, request.ApiKey);

                using var www = new UnityWebRequest(url, request.HttpMethod);
                www.SetApiRequestHeaders(sessionToken, apiSession.MasterVersion);
                if (request.HttpMethod != UnityWebRequest.kHttpVerbGET)
                {
                    www.uploadHandler = new UploadHandlerRaw(MessagePackSerializer.Serialize(request.RequestObject));
                }
                www.downloadHandler = new DownloadHandlerBuffer();
                www.timeout = 60;
                await www.SendWebRequest();

                var response = www.result switch
                {
                    UnityWebRequest.Result.Success => request.DeserializeResponse(www.downloadHandler.data),
                    _ => default
                };
                Debug.Log(string.Format("UnityWebRequest.Result: {1}, errorCode: {1}, networkError: {2}, errorResponse: {2}", www.result, response.ErrorCode, response.NetworkError, response.ErrorMessage));
                return response;
            }
            finally
            {
                loading.HideLoading();
            }
        }

        public async UniTask<TResponse> RequestAsync<TResponse>(RequestBase<TResponse> request) where TResponse : ResponseBase, new() => (TResponse)await RequestAsync((RequestBase)request);
    }
}
