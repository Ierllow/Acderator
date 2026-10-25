using MessagePack;
using System.Collections.Generic;
using System.Linq;

namespace Acderator.Shared
{
    public interface IApiRequest<TResponse> where TResponse : ResponseBase { }

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

    [MessagePackObject]
    public class ResponseHeader
    {
        [Key(0)] public int Code { get; set; }
        [Key(1)] public string Message { get; set; } = string.Empty;
        [Key(2)] public string Token { get; set; } = string.Empty;
        [Key(3)] public Dictionary<string, object> Master { get; set; } = new();
    }

    [MessagePackObject]
    public class ResponseBase
    {
        [Key(0)] public ResponseHeader Header { get; set; } = new();

        [IgnoreMember] public int ErrorCode => Header?.Code ?? -1;
        [IgnoreMember] public string ErrorMessage => Header?.Message ?? string.Empty;
        [IgnoreMember] public Dictionary<string, object> Master => Header?.Master ?? new();
        [IgnoreMember] public NetworkError NetworkError => ErrorCode.ToNetworkError();
        [IgnoreMember] public bool IsSuccess => ErrorCode == 0;
    }

    public static class NetworkErrorExtensions
    {
        public static NetworkError ToNetworkError(this int code) => code switch
        {
            10001 => NetworkError.BadRequest,
            10002 => NetworkError.InvalidRequestFormat,
            11001 => NetworkError.Unauthorized,
            11002 => NetworkError.Forbidden,
            12001 => NetworkError.NotFound,
            12002 => NetworkError.Conflict,
            13001 => NetworkError.PreconditionFailed,
            13002 => NetworkError.Maintenance,
            14001 => NetworkError.UnsupportedMediaType,
            14002 => NetworkError.TooManyRequests,
            14003 => NetworkError.PayloadTooLarge,
            15000 => NetworkError.ServiceFailure,
            15001 => NetworkError.ResponseFailure,
            15002 => NetworkError.DataFailure,
            15003 => NetworkError.ResourceFailure,
            _ => NetworkError.None,
        };
    }

    [MessagePackObject]
    public class LoginRequest : IApiRequest<LoginResponse>
    {
        [Key(0)] public string UserId { get; set; } = string.Empty;
        [Key(1)] public string Password { get; set; } = string.Empty;
    }

    [MessagePackObject]
    public class RegisterRequest : IApiRequest<RegisterResponse>
    {
        [Key(0)] public string Uuid { get; set; } = string.Empty;
    }

    [MessagePackObject]
    public class LogoutRequest : IApiRequest<LogoutResponse> { }

    [MessagePackObject]
    public class UserDataRequest : IApiRequest<UserDataResponse> { }

    [MessagePackObject]
    public class ScoreBeginRequest : IApiRequest<ScoreBeginResponse>
    {
        [Key(0)] public int ScoreId { get; set; }
    }

    [MessagePackObject]
    public class ScoreSubmitRequest : IApiRequest<ScoreSubmitResponse>
    {
        [Key(0)] public string SessionId { get; set; } = string.Empty;
        [Key(1)] public int Score { get; set; }
    }

    [MessagePackObject]
    public class ChartRequest : IApiRequest<ChartResponse>
    {
        [Key(0)] public int Sid { get; set; }
    }

    [MessagePackObject]
    public class LoginResponse : ResponseBase
    {
        [IgnoreMember] public string Token => Header?.Token ?? string.Empty;
    }

    [MessagePackObject]
    public class RegisterResponse : ResponseBase
    {
        [Key(1)] public string Token { get; set; } = string.Empty;
        [Key(2)] public int UserId { get; set; }
        [Key(3)] public string Password { get; set; } = string.Empty;
    }

    [MessagePackObject]
    public class LogoutResponse : ResponseBase { }

    [MessagePackObject]
    public class ScoreBeginResponse : ResponseBase
    {
        [Key(1)] public string SessionId { get; set; } = string.Empty;
    }

    [MessagePackObject]
    public class ScoreSubmitResponse : ResponseBase { }

    [MessagePackObject]
    public class ScoreEntry
    {
        [Key(0)] public int ScoreId { get; set; }
        [Key(1)] public int Score { get; set; }
    }

    [MessagePackObject]
    public class UserDataResponse : ResponseBase
    {
        [Key(1)] public List<ScoreEntry> Scores { get; set; } = new();
        [IgnoreMember] public IEnumerable<(int sid, int score)> ScorePairs => Scores?.Select(x => (x.ScoreId, x.Score)) ?? Enumerable.Empty<(int, int)>();
    }

    [MessagePackObject]
    public class ChartResponse : ResponseBase
    {
        [Key(1)] public int Tempo { get; set; }
        [Key(2)] public List<ChartSpeedChange> SpeedChanges { get; set; } = new();
        [Key(3)] public List<ChartNote> Notes { get; set; } = new();
    }

    [MessagePackObject]
    public class ChartSpeedChange
    {
        [Key(0)] public float Beat { get; set; }
        [Key(1)] public double Speed { get; set; }
    }

    [MessagePackObject]
    public class ChartNote
    {
        [Key(0)] public float BeatBegin { get; set; }
        [Key(1)] public float BeatEnd { get; set; }
        [Key(2)] public int Lane { get; set; }
        [Key(3)] public int NoteType { get; set; }
        [Key(4)] public float CurveDuration { get; set; }
        [Key(5)] public bool UseMidPoint { get; set; }
        [Key(6)] public List<ChartCurveSegment> Segments { get; set; } = new();
    }

    [MessagePackObject]
    public class ChartCurveSegment
    {
        [Key(0)] public float StartTime { get; set; }
        [Key(1)] public float EndTime { get; set; }
        [Key(2)] public List<ChartVector2> Points { get; set; } = new();
    }

    [MessagePackObject]
    public class ChartVector2
    {
        [Key(0)] public float X { get; set; }
        [Key(1)] public float Y { get; set; }
    }
}
