using PlayFab;
using System;

namespace Intense.Api
{
    public static class PlayFabErrorCodeExtensions
    {
        public static string GetErrorMessage(this PlayFabErrorCode code) => code.EnumEquals(PlayFabErrorCode.Success) ? "" : code switch
        {
            PlayFabErrorCode.ConnectionError => "通信エラーが発生しました。",
            PlayFabErrorCode.AccountBanned => "アカウントがBANされています。",
            PlayFabErrorCode.AccountNotFound => "アカウントが見つかりませんでした。",
            PlayFabErrorCode.UsernameNotAvailable => "既に使われている名前です。",
            PlayFabErrorCode.AccountAlreadyLinked or PlayFabErrorCode.LinkedAccountAlreadyClaimed => "既にアカウント連携されています。",
            PlayFabErrorCode.ServerFailedToStart => "サーバーエラーが発生しました。",
            PlayFabErrorCode.DuplicateUsername or PlayFabErrorCode.NameNotAvailable => "使用できない名前です。",
            PlayFabErrorCode.InvalidAccount => "使用できないアカウントです。",
            PlayFabErrorCode.NotAuthenticated or PlayFabErrorCode.NotAuthorized => "認証エラーが発生しました。",
            PlayFabErrorCode.InternalServerError => "サーバーエラーが発生しました。",
            PlayFabErrorCode.UnknownError => "不正な通信です。",
            PlayFabErrorCode.InvalidRequest => "不正なリクエストです。",
            PlayFabErrorCode.InvalidParams => "無効な値が含まれています。",
            PlayFabErrorCode.AccountDeleted => "既に削除されているアカウントです。",
            PlayFabErrorCode.APIClientRequestRateLimitExceeded or PlayFabErrorCode.APIConcurrentRequestLimitExceeded or PlayFabErrorCode.ConcurrentEditError or PlayFabErrorCode.DataUpdateRateExceeded or PlayFabErrorCode.DownstreamServiceUnavailable or PlayFabErrorCode.ServiceUnavailable => "エラーが発生しました。\n しばらくたってからお試しください。",
            _ => "エラーが発生しました。",
        };
    }
}