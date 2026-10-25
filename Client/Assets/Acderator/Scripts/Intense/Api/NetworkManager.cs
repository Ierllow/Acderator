using Acderator.Shared;
using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Intense.UI;
using MagicOnion;
using MagicOnion.Client;
using System;
using UnityEngine;
using Zenject;

namespace Intense.Api
{
    internal class NetworkManager : MonoBehaviour
    {
        [SerializeField] private NetworkConfig networkConfigObject;

        [Inject] private readonly Loading loading;
        [Inject] private readonly IApiSession apiSession;

        private GrpcChannel channel;
        private IAuthService authService;
        private IUserService userService;
        private IScoreService scoreService;
        private IChartService chartService;

        public string Token { set => apiSession.Token = value; }

        private void Awake()
        {
            var address = networkConfigObject.ApiServerUrl.TrimEnd('/');
            channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
            {
                HttpHandler = new YetAnotherHttpHandler
                {
                    Http2Only = address.StartsWith("http://", StringComparison.OrdinalIgnoreCase),
                },
                DisposeHttpClient = true,
            });
            authService = MagicOnionClient.Create<IAuthService>(channel);
            userService = MagicOnionClient.Create<IUserService>(channel);
            scoreService = MagicOnionClient.Create<IScoreService>(channel);
            chartService = MagicOnionClient.Create<IChartService>(channel);
        }

        private void OnDestroy() => channel?.Dispose();

        public async UniTask<TResponse> RequestAsync<TResponse>(IApiRequest<TResponse> request) where TResponse : ResponseBase
        {
            return (TResponse)(ResponseBase)(request switch
            {
                LoginRequest value => await SendAsync(() => authService.WithHeaders(CreateHeaders()).LoginAsync(value)),
                RegisterRequest value => await SendAsync(() => authService.WithHeaders(CreateHeaders()).RegisterAsync(value)),
                LogoutRequest value => await SendAsync(() => authService.WithHeaders(CreateHeaders()).LogoutAsync(value)),
                UserDataRequest value => await SendAsync(() => userService.WithHeaders(CreateHeaders()).GetDataAsync(value)),
                ScoreBeginRequest value => await SendAsync(() => scoreService.WithHeaders(CreateHeaders()).BeginAsync(value)),
                ScoreSubmitRequest value => await SendAsync(() => scoreService.WithHeaders(CreateHeaders()).SubmitAsync(value)),
                ChartRequest value => await SendAsync(() => chartService.WithHeaders(CreateHeaders()).GetAsync(value)),
                _ => throw new ArgumentOutOfRangeException(nameof(request)),
            });
        }

        private async UniTask<TResponse> SendAsync<TResponse>(Func<UnaryResult<TResponse>> request) where TResponse : ResponseBase
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) return default;

            loading.ShowLoading();
            try
            {
                var response = await request();
                Debug.Log(string.Format("MagicOnion response: errorCode={0}, networkError={1}, errorMessage={2}", response?.ErrorCode, response?.NetworkError, response?.ErrorMessage));
                return response;
            }
            catch (RpcException exception)
            {
                Debug.LogWarning(exception);
                return default;
            }
            finally
            {
                loading.HideLoading();
            }
        }

        private Metadata CreateHeaders()
        {
            var headers = new Metadata { { "x-master-version", apiSession.MasterVersion ?? string.Empty } };
            if (!string.IsNullOrEmpty(apiSession.Token)) headers.Add("authorization", string.Format("Bearer {0}", apiSession.Token));
            return headers;
        }
    }
}