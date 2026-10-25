using Acderator.Shared;
using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Intense.UI;
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

        public async UniTask<ResponseBase> RequestAsync(IApiRequest request) => request switch
        {
            LoginRequest value => await RequestAsync(value),
            RegisterRequest value => await RequestAsync(value),
            LogoutRequest value => await RequestAsync(value),
            UserDataRequest value => await RequestAsync(value),
            ScoreBeginRequest value => await RequestAsync(value),
            ScoreSubmitRequest value => await RequestAsync(value),
            ChartRequest value => await RequestAsync(value),
            _ => throw new ArgumentOutOfRangeException(nameof(request)),
        };

        public UniTask<LoginResponse> RequestAsync(LoginRequest request) => SendAsync(async () => await authService.WithHeaders(CreateHeaders()).LoginAsync(request));
        public UniTask<RegisterResponse> RequestAsync(RegisterRequest request) => SendAsync(async () => await authService.WithHeaders(CreateHeaders()).RegisterAsync(request));
        public UniTask<LogoutResponse> RequestAsync(LogoutRequest request) => SendAsync(async () => await authService.WithHeaders(CreateHeaders()).LogoutAsync(request));
        public UniTask<UserDataResponse> RequestAsync(UserDataRequest request) => SendAsync(async () => await userService.WithHeaders(CreateHeaders()).GetDataAsync(request));
        public UniTask<ScoreBeginResponse> RequestAsync(ScoreBeginRequest request) => SendAsync(async () => await scoreService.WithHeaders(CreateHeaders()).BeginAsync(request));
        public UniTask<ScoreSubmitResponse> RequestAsync(ScoreSubmitRequest request) => SendAsync(async () => await scoreService.WithHeaders(CreateHeaders()).SubmitAsync(request));
        public UniTask<ChartResponse> RequestAsync(ChartRequest request) => SendAsync(async () => await chartService.WithHeaders(CreateHeaders()).GetAsync(request));

        private async UniTask<TResponse> SendAsync<TResponse>(Func<UniTask<TResponse>> request) where TResponse : ResponseBase
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