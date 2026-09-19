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

        [Inject] private readonly Loading loading;
        [Inject] private readonly IApiSession apiSession;

        public string Token { set => apiSession.Token = value; }

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
                try
                {
                    await www.SendWebRequest();
                }
                catch (UnityWebRequestException)
                {

                }

                var response = TryDeserializeResponse(request, www);
                Debug.Log(string.Format("UnityWebRequest.Result: {0}, errorCode: {1}, networkError: {2}, errorMessage: {3}", www.result, response?.ErrorCode, response?.NetworkError, response?.ErrorMessage));
                return response;
            }
            finally
            {
                loading.HideLoading();
            }
        }

        private ResponseBase TryDeserializeResponse(RequestBase request, UnityWebRequest www)
        {
            var data = www.downloadHandler?.data;
            if (data == null || data.Length == 0) return default;

            try
            {
                var response = request.DeserializeResponse(data);
                return www.result == UnityWebRequest.Result.Success || !(response?.IsSuccess ?? false) ? response : default;
            }
            catch (MessagePackSerializationException e)
            {
                Debug.LogWarning(e);
                return default;
            }
        }

        public async UniTask<TResponse> RequestAsync<TResponse>(RequestBase<TResponse> request) where TResponse : ResponseBase, new() => (TResponse)await RequestAsync((RequestBase)request);
    }
}