using Acderator.Shared;
using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Intense.UI;
using MagicOnion;
using MagicOnion.Client;
using System;
using System.Collections.Generic;
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
        private readonly Dictionary<Type, object> serviceDict = new();

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
        }

        private void OnDestroy() => channel?.Dispose();

        private TService GetService<TService>() where TService : class, IService<TService>
        {
            if (serviceDict.TryGetValue(typeof(TService), out var service)) return (TService)service;
            var created = MagicOnionClient.Create<TService>(channel);
            serviceDict[typeof(TService)] = created;
            return created;
        }

        public async UniTask<TResponse> RequestAsync<TResponse>(IApiRequest<TResponse> request) where TResponse : ResponseBase
        {
            return (TResponse)(ResponseBase)(request switch
            {
                LoginRequest value => await SendAsync(() => GetService<IAuthService>().WithHeaders(CreateHeaders()).LoginAsync(value)),
                RegisterRequest value => await SendAsync(() => GetService<IAuthService>().WithHeaders(CreateHeaders()).RegisterAsync(value)),
                LogoutRequest value => await SendAsync(() => GetService<IAuthService>().WithHeaders(CreateHeaders()).LogoutAsync(value)),
                UserDataRequest value => await SendAsync(() => GetService<IUserService>().WithHeaders(CreateHeaders()).GetDataAsync(value)),
                ScoreBeginRequest value => await SendAsync(() => GetService<IScoreService>().WithHeaders(CreateHeaders()).BeginAsync(value)),
                ScoreSubmitRequest value => await SendAsync(() => GetService<IScoreService>().WithHeaders(CreateHeaders()).SubmitAsync(value)),
                ChartRequest value => await SendAsync(() => GetService<IChartService>().WithHeaders(CreateHeaders()).GetAsync(value)),
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